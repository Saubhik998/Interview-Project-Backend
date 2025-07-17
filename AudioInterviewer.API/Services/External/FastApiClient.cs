using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AudioInterviewer.API.Services.External
{
    /// <summary>
    /// Configuration settings for external API endpoints.
    /// </summary>
    public class ApiSettings
    {
        /// <summary>
        /// Base URL of the FastAPI backend service.
        /// </summary>
        public string BaseUrl { get; set; } = "http://localhost:8000/api";
    }

    /// <summary>
    /// Client service to communicate with the FastAPI backend for generating and retrieving interview questions.
    /// </summary>
    public class FastApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FastApiClient> _logger;
        private readonly string _baseUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="FastApiClient"/> class.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> used to send requests.</param>
        /// <param name="options">The API settings injected from configuration.</param>
        /// <param name="logger">The logger for logging information and errors.</param>
        public FastApiClient(HttpClient httpClient, IOptions<ApiSettings> options, ILogger<FastApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _baseUrl = options.Value.BaseUrl.TrimEnd('/');
        }

        /// <summary>
        /// Requests the first interview question generated based on the given job description.
        /// </summary>
        /// <param name="jd">The job description text.</param>
        /// <returns>The first interview question as a string. Returns a default question if the call fails.</returns>
        public async Task<string> GetFirstQuestionAsync(string jd)
        {
            var payload = new { jd };
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/generate", payload);
                var responseText = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("FASTAPI /generate response: {Response}", responseText);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("FASTAPI /generate failed: {StatusCode}", response.StatusCode);
                    return "Tell me about yourself.";
                }

                var result = JsonSerializer.Deserialize<GenerateResponse>(responseText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Questions?.FirstOrDefault() ?? "Tell me about yourself.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching first question.");
                return "Tell me about yourself.";
            }
        }

        /// <summary>
        /// Requests the next interview question based on the job description, previous question, and candidate's answer.
        /// </summary>
        /// <param name="jd">The job description text.</param>
        /// <param name="previousQuestion">The previous interview question text.</param>
        /// <param name="answer">The candidate's answer to the previous question.</param>
        /// <returns>The next interview question as a string. Returns a default question if the call fails.</returns>
        public async Task<string> GetNextQuestionAsync(string jd, string previousQuestion, string answer)
        {
            var payload = new
            {
                jd,
                previous_question = previousQuestion,
                answer
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/next-question", payload);
                var responseText = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("FASTAPI /next-question response: {Response}", responseText);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("FASTAPI /next-question failed: {StatusCode}", response.StatusCode);
                    return "What would you like to share next?";
                }

                var result = JsonSerializer.Deserialize<NextQuestionResponse>(responseText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.NextQuestion ?? "What would you like to share next?";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching next question.");
                return "What would you like to share next?";
            }
        }

        /// <summary>
        /// Internal DTO for deserializing generate API response.
        /// </summary>
        private class GenerateResponse
        {
            /// <summary>
            /// List of generated questions.
            /// </summary>
            public List<string> Questions { get; set; } = new();
        }

        /// <summary>
        /// Internal DTO for deserializing next-question API response.
        /// </summary>
        private class NextQuestionResponse
        {
            /// <summary>
            /// The next question text returned from the API.
            /// </summary>
            [JsonPropertyName("next_question")]
            public string NextQuestion { get; set; } = "";
        }
    }
}
