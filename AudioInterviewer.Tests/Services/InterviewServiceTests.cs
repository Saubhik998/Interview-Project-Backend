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
    public class InterviewServiceTests
    {
        private readonly InterviewService _interviewService;

        public InterviewServiceTests()
        {
            // ✅ 1. Use a real implementation of the IApiClient with fake behavior
            var fakeApiClient = new FakeApiClient();

            // ✅ 2. Setup mock HTTP client for report submission
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

            // ✅ 3. Provide dummy settings for TestMongoDbContext
            var settings = Options.Create(new MongoDbSettings
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "TestDB"
            });

            var dbContext = new TestMongoDbContext(settings);

            // ✅ 4. Create InterviewService instance with fake/mock dependencies
            _interviewService = new InterviewService(dbContext, fakeApiClient, httpClientFactory.Object);
        }

        [Fact]
        public async Task InitializeSessionAsync_ShouldInitializeSession()
        {
            await _interviewService.InitializeSessionAsync("Backend Developer", "test@example.com");

            var questions = _interviewService.GetQuestions();
            Assert.Single(questions);
            Assert.Equal("What are your strengths?", questions[0].Text);
            Assert.Equal(0, _interviewService.CurrentIndex);
        }

        [Fact]
        public async Task GetNextQuestionAsync_ShouldReturnNextQuestion()
        {
            await _interviewService.InitializeSessionAsync("Backend Developer", "test@example.com");

            var next = await _interviewService.GetNextQuestionAsync();
            Assert.Equal("What are your strengths?", next);
        }

        [Fact]
        public async Task SubmitAnswer_ShouldStoreAnswer()
        {
            await _interviewService.InitializeSessionAsync("Backend Developer", "test@example.com");

            var result = _interviewService.SubmitAnswer(new AnswerDto
            {
                Question = "What are your strengths?",
                Transcript = "I am detail oriented.",
                AudioBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("fake audio"))
            });

            Assert.True(result);
            Assert.Equal(1, _interviewService.CurrentIndex);
        }

        [Fact]
        public void GetCompletionSummary_ShouldReturnValidJson()
        {
            var summary = _interviewService.GetCompletionSummary();
            var json = JsonSerializer.Serialize(summary);

            Assert.Contains("Interview completed", json);
        }
    }

    // ✅ A simple fake implementation of IApiClient to use in tests
    public class FakeApiClient : IApiClient
    {
        public Task<string> GetFirstQuestionAsync(string jd)
        {
            return Task.FromResult("What are your strengths?");
        }

        public Task<string> GetNextQuestionAsync(string jd, string previousQuestion, string answer)
        {
            return Task.FromResult("Tell me about a challenging project.");
        }
    }
}
