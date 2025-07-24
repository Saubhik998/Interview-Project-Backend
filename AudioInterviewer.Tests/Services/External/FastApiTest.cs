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

/// <summary>
/// Unit tests for <see cref="FastApiClient"/> covering external API interactions, 
/// success and fallback paths for question generation.
/// </summary>
public class FastApiClientTests
{
    private readonly Mock<ILogger<FastApiClient>> _mockLogger;
    private readonly HttpClient _httpClient;
    private readonly FastApiClient _client;

    /// <summary>
    /// Initializes a new instance of <see cref="FastApiClientTests"/>,
    /// configuring the client and logger mocks.
    /// </summary>
    public FastApiClientTests()
    {
        _mockLogger = new Mock<ILogger<FastApiClient>>();
        var mockOptions = Options.Create(new ApiSettings { BaseUrl = "http://test-api.com/api" });
        _httpClient = new HttpClient(new MockHttpMessageHandler());

        _client = new FastApiClient(_httpClient, mockOptions, _mockLogger.Object);
    }

    /// <summary>
    /// Tests that <see cref="FastApiClient.GetFirstQuestionAsync"/> returns a generated question
    /// when the API responds successfully.
    /// </summary>
    [Fact]
    public async Task GetFirstQuestionAsync_ReturnsQuestion_WhenApiSucceeds()
    {
        var result = await _client.GetFirstQuestionAsync("JD");
        Assert.Equal("Generated question 1", result);
    }

    /// <summary>
    /// Tests that <see cref="FastApiClient.GetFirstQuestionAsync"/> returns a fallback question
    /// when the API call fails.
    /// </summary>
    [Fact]
    public async Task GetFirstQuestionAsync_ReturnsFallback_WhenApiFails()
    {
        var client = CreateClientWithStatus(HttpStatusCode.InternalServerError, "{\"questions\":[]}");
        var result = await client.GetFirstQuestionAsync("JD");
        Assert.Equal("Tell me about yourself.", result);
    }

    /// <summary>
    /// Tests that <see cref="FastApiClient.GetNextQuestionAsync"/> returns a next question
    /// when the API responds successfully.
    /// </summary>
    [Fact]
    public async Task GetNextQuestionAsync_ReturnsNextQuestion_WhenApiSucceeds()
    {
        var result = await _client.GetNextQuestionAsync("JD", "Q1", "A1");
        Assert.Equal("Next generated question", result);
    }

    /// <summary>
    /// Tests that <see cref="FastApiClient.GetNextQuestionAsync"/> returns a fallback question
    /// when the API call fails.
    /// </summary>
    [Fact]
    public async Task GetNextQuestionAsync_ReturnsFallback_WhenApiFails()
    {
        var client = CreateClientWithStatus(HttpStatusCode.BadRequest, "{}");
        var result = await client.GetNextQuestionAsync("JD", "Q1", "A1");
        Assert.Equal("What would you like to share next?", result);
    }

    /// <summary>
    /// Helper to create a <see cref="FastApiClient"/> with a custom HTTP status code and response content.
    /// </summary>
    /// <param name="statusCode">HTTP status code to simulate.</param>
    /// <param name="content">Response body as JSON string.</param>
    /// <returns>A new <see cref="FastApiClient"/> with the mock handler.</returns>
    private FastApiClient CreateClientWithStatus(HttpStatusCode statusCode, string content)
    {
        var handler = new CustomMockHandler(statusCode, content);
        var httpClient = new HttpClient(handler);
        var mockOptions = Options.Create(new ApiSettings { BaseUrl = "http://test-api.com/api" });
        return new FastApiClient(httpClient, mockOptions, _mockLogger.Object);
    }

    /// <summary>
    /// Mocks API responses for FastApiClient based on the requested endpoint for normal (200 OK) scenarios.
    /// </summary>
    private class MockHttpMessageHandler : HttpMessageHandler
    {
        /// <summary>
        /// Returns a canned response for /generate and other endpoints.
        /// </summary>
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

    /// <summary>
    /// Message handler for simulating API errors or specific response bodies.
    /// </summary>
    private class CustomMockHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _content;

        /// <summary>
        /// Initializes a new handler for a specific status code and content.
        /// </summary>
        /// <param name="statusCode">HTTP status code to simulate.</param>
        /// <param name="content">Response body as JSON string.</param>
        public CustomMockHandler(HttpStatusCode statusCode, string content)
        {
            _statusCode = statusCode;
            _content = content;
        }

        /// <summary>
        /// Returns a <see cref="HttpResponseMessage"/> with the specified status and content.
        /// </summary>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_content, Encoding.UTF8, "application/json")
            });
        }
    }
}
