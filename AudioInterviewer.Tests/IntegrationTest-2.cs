// using System.Net;
// using System.Net.Http.Json;
// using System.Text;
// using System.Text.Json;
// using System.Threading.Tasks;
// using FluentAssertions;
// using Microsoft.AspNetCore.Mvc.Testing;
// using Xunit;

// namespace AudioInterviewer.API.Tests.Integration
// {
//     public class AudioInterviewApiTests : IClassFixture<WebApplicationFactory<Program>>
//     {
//         private readonly WebApplicationFactory<Program> _factory;
//         private readonly HttpClient _client;

//         public AudioInterviewApiTests(WebApplicationFactory<Program> factory)
//         {
//             _factory = factory.WithWebHostBuilder(builder =>
//             {
//                 // Optional: configure test services here
//             });
//             _client = _factory.CreateClient();
//         }

//         [Fact]
//         public async Task HealthCheck_Should_Return_Healthy()
//         {
//             var resp = await _client.GetAsync("/api/interview/health");
//             resp.EnsureSuccessStatusCode();
//             (await resp.Content.ReadAsStringAsync()).Should().Be("Healthy");
//         }

//         [Fact]
//         public async Task InterviewInit_Should_Validate_Input()
//         {
//             var resp = await _client.PostAsJsonAsync("/api/interview/init", new { email = "bad", jobDescription = "short" });
//             resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

//             resp = await _client.PostAsJsonAsync("/api/interview/init", new { jobDescription = "A decently long job description." });
//             resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

//             resp = await _client.PostAsJsonAsync("/api/interview/init", new { email = "a@test.com" });
//             resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

//             var okResp = await _client.PostAsJsonAsync("/api/interview/init", new { email = "a@test.com", jobDescription = "A decently long job description." });
//             okResp.StatusCode.Should().Be(HttpStatusCode.OK);
//         }

//         [Fact]
//         public async Task GetNextQuestion_BadSessionId_Should_Return_BadRequest_NotFound_Or_500()
//         {
//             var resp = await _client.GetAsync("/api/interview/question");
//             resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

//             resp = await _client.GetAsync("/api/interview/question?sessionId=invalid_doesnt_exist");
//             resp.StatusCode.Should().Match(s =>
//                 s == HttpStatusCode.NotFound ||
//                 s == HttpStatusCode.OK ||
//                 s == HttpStatusCode.InternalServerError
//             );
//         }

//         [Fact]
//         public async Task GetNextQuestion_While_Complete_Returns_InterviewComplete()
//         {
//             var initResp = await _client.PostAsJsonAsync("/api/interview/init", new { email = "steps@test.com", jobDescription = "Long enough job description to pass." });
//             var sessionId = JsonDocument.Parse(await initResp.Content.ReadAsStringAsync()).RootElement.GetProperty("sessionId").GetString();

//             for (int i = 0; i < 6; i++)
//             {
//                 var qResp = await _client.GetAsync($"/api/interview/question?sessionId={sessionId}");
//                 qResp.StatusCode.Should().Be(HttpStatusCode.OK);
//                 var body = await qResp.Content.ReadAsStringAsync();
//                 if (body.Contains("Interview complete"))
//                     return;
//             }
//         }

//         [Fact]
//         public async Task SubmitAnswer_Should_Validate_All_Bad_Payloads()
//         {
//             var resp = await _client.PostAsJsonAsync("/api/interview/answer", new { });
//             resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

//             resp = await _client.PostAsJsonAsync("/api/interview/answer", new { sessionId = "", question = "" });
//             resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

//             var init = await _client.PostAsJsonAsync("/api/interview/init", new { email = "dto@bad.com", jobDescription = "Some long job desc." });
//             var sessionId = JsonDocument.Parse(await init.Content.ReadAsStringAsync()).RootElement.GetProperty("sessionId").GetString();

//             var tooShortQ = await _client.PostAsJsonAsync("/api/interview/answer", new
//             {
//                 sessionId,
//                 question = "short",
//                 audioBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(new string('a', 5100))),
//                 transcript = "okay"
//             });
//             tooShortQ.StatusCode.Should().Be(HttpStatusCode.BadRequest);

//             var shortAudio = await _client.PostAsJsonAsync("/api/interview/answer", new
//             {
//                 sessionId,
//                 question = "What is your experience with dotnet?",
//                 audioBase64 = "too-short",
//                 transcript = "t"
//             });
//             shortAudio.StatusCode.Should().Be(HttpStatusCode.BadRequest);

//             var longTranscript = new string('a', 6000);
//             var respLongTranscript = await _client.PostAsJsonAsync("/api/interview/answer", new
//             {
//                 sessionId,
//                 question = "What is your experience with dotnet?",
//                 audioBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(new string('a', 5100))),
//                 transcript = longTranscript
//             });
//             respLongTranscript.StatusCode.Should().Be(HttpStatusCode.BadRequest);

//             var respMalformed = await _client.PostAsJsonAsync("/api/interview/answer", new
//             {
//                 sessionId,
//                 question = "What is your experience with dotnet?",
//                 audioBase64 = "not_base64_$$$",
//                 transcript = "t"
//             });
//             respMalformed.StatusCode.Should().Be(HttpStatusCode.BadRequest);
//         }

//         [Fact]
//         public async Task AudioUpload_TooLarge_Should_Return_BadRequest()
//         {
//             var initRequest = new
//             {
//                 email = "largeaudio@example.com",
//                 jobDescription = "Another engineer role"
//             };
//             var initResp = await _client.PostAsJsonAsync("/api/interview/init", initRequest);
//             var sessionId = JsonDocument.Parse(await initResp.Content.ReadAsStringAsync())
//                 .RootElement.GetProperty("sessionId").GetString();

//             string bigAudio = Convert.ToBase64String(new byte[6 * 1024 * 1024]);
//             var answer = new
//             {
//                 sessionId,
//                 question = "Any?",
//                 audioBase64 = bigAudio,
//                 transcript = "big audio"
//             };
//             var resp = await _client.PostAsJsonAsync("/api/interview/answer", answer);
//             resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
//         }

//         [Fact]
//         public async Task CompleteInterview_Should_Handle_Invalid_And_DoubleCompletion()
//         {
//             var resp = await _client.PostAsync("/api/interview/complete", null);
//             resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

//             var init = await _client.PostAsJsonAsync("/api/interview/init", new { email = "comp@ex.com", jobDescription = "A real job." });
//             var sessionId = JsonDocument.Parse(await init.Content.ReadAsStringAsync()).RootElement.GetProperty("sessionId").GetString();

//             var ans = new
//             {
//                 sessionId,
//                 question = "What is your experience with dotnet?",
//                 audioBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(new string('a', 5100))),
//                 transcript = "This is an answer"
//             };
//             await _client.PostAsJsonAsync("/api/interview/answer", ans);

//             var first = await _client.PostAsync($"/api/interview/complete?sessionId={sessionId}", null);
//             first.StatusCode.Should().Be(HttpStatusCode.OK);

//             var second = await _client.PostAsync($"/api/interview/complete?sessionId={sessionId}", null);
//             second.StatusCode.Should().Be(HttpStatusCode.OK);
//         }

//         [Fact]
//         public async Task GetReport_Missing_Or_Invalid_SessionId_Should_Return_BadRequest()
//         {
//             var resp = await _client.GetAsync("/api/interview/report");
//             resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

//             var resp2 = await _client.GetAsync("/api/interview/report?sessionId=garbage");
//             resp2.StatusCode.Should().Match(x =>
//                 x == HttpStatusCode.NotFound ||
//                 x == HttpStatusCode.InternalServerError ||
//                 x == HttpStatusCode.BadRequest
//             );
//         }

//         [Fact]
//         public async Task GetReportsByEmail_Should_Validate_Input_And_Handle_Empty()
//         {
//             var resp = await _client.GetAsync("/api/interview/reports");
//             resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

//             var resp2 = await _client.GetAsync("/api/interview/reports?email=not-an-email");
//             resp2.StatusCode.Should().Be(HttpStatusCode.OK);

//             var resp3 = await _client.GetAsync("/api/interview/reports?email=none@missing.com");
//             resp3.StatusCode.Should().Be(HttpStatusCode.OK);
//             var arr = await resp3.Content.ReadAsStringAsync();
//             arr.Should().Contain("[");
//         }

//         [Fact]
//         public async Task AudioDownload_Should_Return_NotFound_And_500_For_Bad_Ids()
//         {
//             var fakeOid = "507f1f77bcf86cd799439011";
//             var resp = await _client.GetAsync($"/api/audio/{fakeOid}");
//             resp.StatusCode.Should().Match(s => s == HttpStatusCode.NotFound || s == HttpStatusCode.InternalServerError);

//             var resp2 = await _client.GetAsync("/api/audio/not_an_oid");
//             resp2.StatusCode.Should().Match(x => x == HttpStatusCode.InternalServerError || x == HttpStatusCode.BadRequest);
//         }

//         [Fact]
//         public async Task SubmitAnswer_NonExistingSession_Should_Return_Error()
//         {
//             var answerReq = new
//             {
//                 sessionId = "bad-session-id-not-in-db",
//                 question = "Any?",
//                 audioBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(new string('a', 5100))),
//                 transcript = "testing"
//             };
//             var resp = await _client.PostAsJsonAsync("/api/interview/answer", answerReq);
//             resp.StatusCode.Should().Match(x => x == HttpStatusCode.NotFound || x == HttpStatusCode.BadRequest || x == HttpStatusCode.InternalServerError);
//         }

//         [Fact]
//         public async Task DownloadAudio_CompletelyMissingId_Should_Be_Handled()
//         {
//             var resp = await _client.GetAsync("/api/audio/__not_even_an_objectid__123456789__");
//             resp.StatusCode.Should().Match(x => x == HttpStatusCode.BadRequest || x == HttpStatusCode.InternalServerError);
//         }
//     }
// }
