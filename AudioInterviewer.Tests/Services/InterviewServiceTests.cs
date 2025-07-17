using AudioInterviewer.API.Models;
using AudioInterviewer.API.Services;
using AudioInterviewer.API.Services.External;
using AudioInterviewer.Tests.Data;
using AudioInterviewer.API.Data;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;
using Microsoft.Extensions.Options;

namespace AudioInterviewer.Tests.Services
{
    /// <summary>
    /// Unit tests for the InterviewService, validating interview session flow and external interactions.
    /// </summary>
    public class InterviewServiceTests
    {
        private readonly InterviewService _interviewService;

        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewServiceTests"/> class with mocked dependencies.
        /// </summary>
        public InterviewServiceTests()
        {
            // 1. Use a fake implementation of IApiClient
            var fakeApiClient = new FakeApiClient();

            // 2. Setup a mock HTTP client for the report evaluation API
            var mockHttpHandler = new Mock<HttpMessageHandler>();
            mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        "{\"score\": 85, \"jd\": \"JD\", \"questions\": [\"Q1\"], \"strengths\": [\"Clarity\"], \"improvements\": [\"Depth\"], \"followUps\": [\"Ask about scalability\"]}",
                        Encoding.UTF8,
                        "application/json")
                });

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory
                .Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(mockHttpHandler.Object));

            // 3. Use a test MongoDb context with dummy configuration
            var settings = Options.Create(new MongoDbSettings
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "TestDB"
            });

            var dbContext = new TestMongoDbContext(settings);

            // 4. Instantiate InterviewService with mocked dependencies
            _interviewService = new InterviewService(dbContext, fakeApiClient, httpClientFactory.Object);
        }

        /// <summary>
        /// Tests whether InitializeSessionAsync correctly sets up a session with questions.
        /// </summary>
        [Fact]
        public async Task InitializeSessionAsync_ShouldInitializeSession()
        {
            await _interviewService.InitializeSessionAsync("Backend Developer", "test@example.com");

            var questions = _interviewService.GetQuestions();
            Assert.Single(questions);
            Assert.Equal("What are your strengths?", questions[0].Text);
            Assert.Equal(0, _interviewService.CurrentIndex);
        }

        /// <summary>
        /// Tests whether GetNextQuestionAsync returns the correct next question.
        /// </summary>
        [Fact]
        public async Task GetNextQuestionAsync_ShouldReturnNextQuestion()
        {
            await _interviewService.InitializeSessionAsync("Backend Developer", "test@example.com");

            var next = await _interviewService.GetNextQuestionAsync();
            Assert.Equal("What are your strengths?", next);
        }

        /// <summary>
        /// Tests whether SubmitAnswerAsync successfully stores an answer and increments the index.
        /// </summary>
        [Fact]
        public async Task SubmitAnswerAsync_ShouldStoreAnswer()
        {
            await _interviewService.InitializeSessionAsync("Backend Developer", "test@example.com");

            var audioBytes = Encoding.UTF8.GetBytes("fake audio");
            var base64Audio = Convert.ToBase64String(audioBytes);

            var result = await _interviewService.SubmitAnswerAsync(new AnswerDto
            {
                Question = "What are your strengths?",
                Transcript = "I am detail oriented.",
                AudioBase64 = base64Audio
            });

            Assert.True(result);
            Assert.Equal(1, _interviewService.CurrentIndex);
        }

        /// <summary>
        /// Tests whether GetCompletionSummaryAsync returns a valid JSON-formatted summary.
        /// </summary>
        [Fact]
        public async Task GetCompletionSummaryAsync_ShouldReturnValidSummary()
        {
            await _interviewService.InitializeSessionAsync("Backend Developer", "test@example.com");

            var summary = await _interviewService.GetCompletionSummaryAsync();

            var json = JsonSerializer.Serialize(summary);
            Assert.Contains("Interview completed", json);
        }
    }

    /// <summary>
    /// A simple in-memory implementation of IApiClient used for testing InterviewService.
    /// </summary>
    public class FakeApiClient : IApiClient
    {
        /// <summary>
        /// Returns a static first question.
        /// </summary>
        public Task<string> GetFirstQuestionAsync(string jd)
        {
            return Task.FromResult("What are your strengths?");
        }

        /// <summary>
        /// Returns a static follow-up question based on the previous answer.
        /// </summary>
        public Task<string> GetNextQuestionAsync(string jd, string previousQuestion, string answer)
        {
            return Task.FromResult("Tell me about a challenging project.");
        }
    }
}
