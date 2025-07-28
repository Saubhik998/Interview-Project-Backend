using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System.Net.Http;
using System.Net;
using System.Text.Json;
using System.Text;
using System.IO;
using AudioInterviewer.API.Models;
using AudioInterviewer.API.Services;
using AudioInterviewer.API.Services.External;
using AudioInterviewer.API.Data;

/// <summary>
/// Unit tests for <see cref="InterviewService"/> logic, database, and integration behavior.
/// </summary>
public class InterviewServiceTests
{
    private readonly Mock<IMongoDbContext> _mockDbContext;
    private readonly Mock<IApiClient> _mockApiClient;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<IMongoCollection<InterviewSession>> _mockSessionCollection;
    private readonly Mock<IMongoCollection<InterviewReport>> _mockReportCollection;
    private readonly Mock<IGridFSBucket> _mockGridFsBucket;
    private readonly InterviewService _service;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes the mocks and the <see cref="InterviewService"/> for testing.
    /// </summary>
    public InterviewServiceTests()
    {
        _mockDbContext = new Mock<IMongoDbContext>();
        _mockApiClient = new Mock<IApiClient>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockSessionCollection = new Mock<IMongoCollection<InterviewSession>>();
        _mockReportCollection = new Mock<IMongoCollection<InterviewReport>>();
        _mockGridFsBucket = new Mock<IGridFSBucket>();
        _httpClient = new HttpClient(new MockHttpMessageHandler());

        _mockDbContext.SetupGet(x => x.Sessions).Returns(_mockSessionCollection.Object);
        _mockDbContext.SetupGet(x => x.Reports).Returns(_mockReportCollection.Object);
        _mockDbContext.SetupGet(x => x.GridFsBucket).Returns(_mockGridFsBucket.Object);
        _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(_httpClient);

        _service = new InterviewService(
            _mockDbContext.Object,
            _mockApiClient.Object,
            _mockHttpClientFactory.Object
        );
    }

    /// <summary>
    /// Verifies that <see cref="InterviewService.InitializeSessionAsync"/> inserts a new session
    /// with the correct question and returns the session ID.
    /// </summary>
    [Fact]
    public async Task InitializeSessionAsync_ShouldInsertSessionAndReturnId()
    {
        var jobDescription = "JD";
        var email = "test@example.com";
        var firstQuestion = "Tell me about yourself.";

        _mockApiClient.Setup(x => x.GetFirstQuestionAsync(It.IsAny<string>())).ReturnsAsync(firstQuestion);

        InterviewSession capturedSession = null!;
        _mockSessionCollection
            .Setup(x => x.InsertOneAsync(It.IsAny<InterviewSession>(), It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()))
            .Callback<InterviewSession, InsertOneOptions, CancellationToken>((s, _, _) => capturedSession = s)
            .Returns(Task.CompletedTask);

        var sessionId = await _service.InitializeSessionAsync(jobDescription, email);

        Assert.NotNull(sessionId);
        Assert.NotNull(capturedSession);
        Assert.Equal(firstQuestion, capturedSession.Questions[0].Text);
        Assert.Equal(email.ToLowerInvariant(), capturedSession.Email);
    }

    /// <summary>
    /// Verifies that <see cref="InterviewService.GetNextQuestionAsync"/> returns the first question
    /// when the current index is zero.
    /// </summary>
    [Fact]
    public async Task GetNextQuestionAsync_ReturnsFirstQuestion_WhenCurrentIndexIsZero()
    {
        var sessionId = "session123";
        var session = new InterviewSession
        {
            Id = sessionId,
            CurrentIndex = 0,
            Questions = new List<Question> { new Question { Text = "Q1" } },
            Answers = new List<Answer>()
        };

        SetupSessionFind(sessionId, session);

        var result = await _service.GetNextQuestionAsync(sessionId);

        Assert.Equal("Q1", result);
    }

    /// <summary>
    /// Verifies that <see cref="InterviewService.SubmitAnswerAsync"/> stores audio data,
    /// adds the answer, and increments the session index.
    /// </summary>
    [Fact]
    public async Task SubmitAnswerAsync_StoresAudioAndUpdatesSession()
    {
        var sessionId = "session123";
        var session = new InterviewSession
        {
            Id = sessionId,
            CurrentIndex = 0,
            Questions = new List<Question> { new Question { Text = "Q1" } },
            Answers = new List<Answer>()
        };

        SetupSessionFind(sessionId, session);

        var audioBytes = new byte[3800];
        new Random().NextBytes(audioBytes);
        var base64 = Convert.ToBase64String(audioBytes); ;

        var answerDto = new AnswerDto
        {
            SessionId = sessionId,
            AudioBase64 = base64,
            Question = "Q1",
            Transcript = "Answer text"
        };

        _mockGridFsBucket
            .Setup(x => x.UploadFromStreamAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<GridFSUploadOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MongoDB.Bson.ObjectId.GenerateNewId());

        _mockSessionCollection
            .Setup(x => x.ReplaceOneAsync(
                It.IsAny<FilterDefinition<InterviewSession>>(),
                It.IsAny<InterviewSession>(),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ReplaceOneResult>(r => r.IsAcknowledged == true && r.ModifiedCount == 1));

        var result = await _service.SubmitAnswerAsync(sessionId, answerDto);

        Assert.True(result);
        Assert.Single(session.Answers);
        Assert.Equal(1, session.CurrentIndex);
    }

    /// <summary>
    /// Verifies that <see cref="InterviewService.GetCompletionSummaryAsync"/> returns the correct summary object.
    /// </summary>
    [Fact]
    public async Task GetCompletionSummaryAsync_ReturnsSummary()
    {
        var sessionId = "s1";
        var session = new InterviewSession
        {
            Id = sessionId,
            Questions = new List<Question> { new Question { Text = "Q1" } },
            Answers = new List<Answer> { new Answer { Question = "Q1", AudioUrl = "url", Transcript = "txt" } }
        };

        SetupSessionFind(sessionId, session);

        var result = await _service.GetCompletionSummaryAsync(sessionId);

        var json = JsonSerializer.Serialize(result);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal(1, root.GetProperty("totalQuestions").GetInt32());
        Assert.Equal(1, root.GetProperty("totalAnswers").GetInt32());
    }

    /// <summary>
    /// Sets up the .FindAsync() mock for the session collection to yield the given session.
    /// </summary>
    /// <param name="sessionId">Session ID that will be searched.</param>
    /// <param name="session">Session object to return.</param>
    private void SetupSessionFind(string sessionId, InterviewSession session)
    {
        var cursor = new Mock<IAsyncCursor<InterviewSession>>();
        cursor.SetupSequence(x => x.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);
        cursor.Setup(x => x.Current).Returns(new List<InterviewSession> { session });

        _mockSessionCollection
            .Setup(x => x.FindAsync<InterviewSession>(It.IsAny<FilterDefinition<InterviewSession>>(), It.IsAny<FindOptions<InterviewSession, InterviewSession>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursor.Object);
    }

    /// <summary>
    /// A test message handler that returns a canned JSON response for evaluation API calls.
    /// </summary>
    private class MockHttpMessageHandler : HttpMessageHandler
    {
        /// <summary>
        /// Synchronously returns a successful JSON response simulating an LLM evaluation.
        /// </summary>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(new
            {
                score = 80,
                strengths = new[] { "Communication", "Problem-solving" },
                improvements = new[] { "Time management" },
                followUps = new[] { "Can you give a specific example?" },
                jd = "JD",
                questions = new[] { "Q1", "Q2" }
            });

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }
    }
    
    [Fact]
    public async Task GetNextQuestionAsync_ReturnsNull_WhenSessionNotFound()
    {
    SetupSessionFind("bad_id", null!);
    var result = await _service.GetNextQuestionAsync("bad_id");
    Assert.Null(result); // or default
    }
}