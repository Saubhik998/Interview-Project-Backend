using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using AudioInterviewer.API;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AudioInterviewer.Tests
{
    public class InterviewIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public InterviewIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact(DisplayName = "Should initialize interview and return first question")]
        public async Task InitializeInterview_ReturnsSessionAndFirstQuestion()
        {
            var payload = new
            {
                email = "test@example.com",
                jobDescription = "We are looking for a skilled software engineer with experience in .NET and cloud computing."
            };

            var response = await _client.PostAsJsonAsync("/api/interview/init", payload);

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.True(json.TryGetProperty("sessionId", out _));
            Assert.True(json.TryGetProperty("firstQuestion", out var question));
            Assert.False(string.IsNullOrWhiteSpace(question.GetString()));
        }

        [Fact(DisplayName = "Should retrieve the next question")]
        public async Task GetNextQuestion_ReturnsNextQuestionOrComplete()
        {
            var initPayload = new
            {
                email = "test@example.com",
                jobDescription = "Job requiring analytical and programming skills."
            };

            var initResponse = await _client.PostAsJsonAsync("/api/interview/init", initPayload);
            initResponse.EnsureSuccessStatusCode();
            var initJson = await initResponse.Content.ReadFromJsonAsync<JsonElement>();
            var sessionId = initJson.GetProperty("sessionId").GetString();

            var questionResponse = await _client.GetAsync($"/api/interview/question?sessionId={sessionId}");
            questionResponse.EnsureSuccessStatusCode();

            var questionJson = await questionResponse.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(questionJson.TryGetProperty("question", out _) || questionJson.TryGetProperty("message", out _));
        }

        [Fact(DisplayName = "Should submit an answer and receive next question or completion message")]
        public async Task SubmitAnswer_ReturnsNextQuestionOrCompletion()
        {
            var initPayload = new
            {
                email = "test@example.com",
                jobDescription = "Entry-level data analyst job."
            };

            var initResponse = await _client.PostAsJsonAsync("/api/interview/init", initPayload);
            initResponse.EnsureSuccessStatusCode();
            var initJson = await initResponse.Content.ReadFromJsonAsync<JsonElement>();
            var sessionId = initJson.GetProperty("sessionId").GetString();
            var firstQuestion = initJson.GetProperty("firstQuestion").GetString();

            var validWavBase64 = Convert.ToBase64String(new byte[4000]).PadRight(5000, 'A');

            var answerPayload = new
            {
                sessionId = sessionId,
                question = firstQuestion,
                transcript = "I enjoy analyzing data to find trends and patterns.",
                audioBase64 = validWavBase64
            };

            var answerResponse = await _client.PostAsJsonAsync("/api/interview/answer", answerPayload);
            var body = await answerResponse.Content.ReadAsStringAsync();

            Assert.True(answerResponse.IsSuccessStatusCode, $"Status: {answerResponse.StatusCode}, Body: {body}");

            var answerJson = JsonDocument.Parse(body).RootElement;
            Assert.True(answerJson.TryGetProperty("question", out _) || answerJson.TryGetProperty("message", out _));
        }
    }
}
