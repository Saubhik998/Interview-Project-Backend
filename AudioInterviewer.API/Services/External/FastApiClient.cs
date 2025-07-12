using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace AudioInterviewer.API.Services.External
{
    public class FastApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:8000/api";

        public FastApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetFirstQuestionAsync(string jd)
        {
            var payload = new { jd };
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/generate", payload);

            var responseText = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[FASTAPI] /generate response: {responseText}");

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[FASTAPI ERROR] Status: {response.StatusCode}");
                return "Tell me about yourself.";
            }

            try
            {
                var result = JsonSerializer.Deserialize<GenerateResponse>(responseText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Questions?.FirstOrDefault() ?? "Tell me about yourself.";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[FASTAPI ERROR] Deserialization failed: {ex.Message}");
                return "Tell me about yourself.";
            }
        }

        public async Task<string> GetNextQuestionAsync(string jd, string previousQuestion, string answer)
        {
            var payload = new
            {
                jd,
                previous_question = previousQuestion,
                answer
            };

            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/next-question", payload);
            var responseText = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[FASTAPI] /next-question response: {responseText}");

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[FASTAPI ERROR] Status: {response.StatusCode}");
                return "What would you like to share next?";
            }

            try
            {
                var result = JsonSerializer.Deserialize<NextQuestionResponse>(responseText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.NextQuestion ?? "What would you like to share next?";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[FASTAPI ERROR] Deserialization failed: {ex.Message}");
                return "What would you like to share next?";
            }
        }

        private class GenerateResponse
        {
            public List<string> Questions { get; set; } = new();
        }

        private class NextQuestionResponse
        {
            [JsonPropertyName("next_question")]
            public string NextQuestion { get; set; } = "";
        }
    }
}
