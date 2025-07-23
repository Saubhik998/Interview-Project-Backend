using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using AudioInterviewer.API.Services.External;

public class FastApiClientTests
{
    private readonly Mock<ILogger<FastApiClient>> _mockLogger;
    private readonly HttpClient _httpClient;
    private readonly FastApiClient _client;

    public FastApiClientTests()
    {
        _mockLogger = new Mock<ILogger<FastApiClient>>();
        var mockOptions = Options.Create(new ApiSettings { BaseUrl = "http://test-api.com/api" });
        _httpClient = new HttpClient(new MockHttpMessageHandler());

        _client = new FastApiClient(_httpClient, mockOptions, _mockLogger.Object);
    }

    [Fact]
    public async Task GetFirstQuestionAsync_ReturnsQuestion_WhenApiSucceeds()
    {
        var result = await _client.GetFirstQuestionAsync("JD");
        Assert.Equal("Generated question 1", result);
    }

    [Fact]
    public async Task GetFirstQuestionAsync_ReturnsFallback_WhenApiFails()
    {
        var client = CreateClientWithStatus(HttpStatusCode.InternalServerError, "{\"questions\":[]}");
        var result = await client.GetFirstQuestionAsync("JD");
        Assert.Equal("Tell me about yourself.", result);
    }

    [Fact]
    public async Task GetNextQuestionAsync_ReturnsNextQuestion_WhenApiSucceeds()
    {
        var result = await _client.GetNextQuestionAsync("JD", "Q1", "A1");
        Assert.Equal("Next generated question", result);
    }

    [Fact]
    public async Task GetNextQuestionAsync_ReturnsFallback_WhenApiFails()
    {
        var client = CreateClientWithStatus(HttpStatusCode.BadRequest, "{}");
        var result = await client.GetNextQuestionAsync("JD", "Q1", "A1");
        Assert.Equal("What would you like to share next?", result);
    }

    // Helper to simulate API response with specific status code
    private FastApiClient CreateClientWithStatus(HttpStatusCode statusCode, string content)
    {
        var handler = new CustomMockHandler(statusCode, content);
        var httpClient = new HttpClient(handler);
        var mockOptions = Options.Create(new ApiSettings { BaseUrl = "http://test-api.com/api" });
        return new FastApiClient(httpClient, mockOptions, _mockLogger.Object);
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string responseJson = request.RequestUri!.AbsolutePath.EndsWith("/generate")
                ? "{\"questions\": [\"Generated question 1\"]}"
                : "{\"next_question\": \"Next generated question\"}";

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });
        }
    }

    private class CustomMockHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _content;

        public CustomMockHandler(HttpStatusCode statusCode, string content)
        {
            _statusCode = statusCode;
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_content, Encoding.UTF8, "application/json")
            });
        }
    }
}
