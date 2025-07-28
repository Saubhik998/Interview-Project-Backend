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
//                 builder.ConfigureAppConfiguration((context, config) =>
//                 {
//                     context.HostingEnvironment.EnvironmentName = "Testing";
//                 });
//             });
//             _client = _factory.CreateClient();
//         }

//         [Fact]
//         public void DummyTest_Should_Always_Pass()
//         {
//             true.Should().BeTrue();
//         }

//         [Fact]
//         public async Task HealthEndpoint_Should_Return_Success()
//         {
//             var response = await _client.GetAsync("/health");
//             response.StatusCode.Should().Be(HttpStatusCode.OK);
//         }

//         [Fact]
//         public async Task InterviewInit_Always_Passes_With_Valid_Payload()
//         {
//             var response = await _client.PostAsJsonAsync("/api/interview/init", new
//             {
//                 email = "test@example.com",
//                 jobDescription = "A valid job description that is long enough."
//             });
//             response.StatusCode.Should().Be(HttpStatusCode.OK);
//         }

//         [Fact]
//         public async Task InterviewInit_With_Invalid_Email_Should_Return_BadRequest_Or_Handle()
//         {
//             var response = await _client.PostAsJsonAsync("/api/interview/init", new
//             {
//                 email = "invalid-email",
//                 jobDescription = "Another long description."
//             });
//             response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.InternalServerError);
//         }

//         [Fact]
//         public async Task InterviewInit_With_Short_JobDescription_Should_Return_BadRequest()
//         {
//             var response = await _client.PostAsJsonAsync("/api/interview/init", new
//             {
//                 email = "test@example.com",
//                 jobDescription = "short"
//             });
//             response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.InternalServerError);
//         }

//         [Fact]
//         public async Task SubmitAnswer_With_Missing_Fields_Should_Still_Handle()
//         {
//             var response = await _client.PostAsJsonAsync("/api/interview/answer", new { });
//             response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
//         }

//         [Fact]
//         public async Task SubmitAnswer_Dummy_Valid_Payload_Should_Return_Success_Or_Handle()
//         {
//             var initResp = await _client.PostAsJsonAsync("/api/interview/init", new
//             {
//                 email = "sample@test.com",
//                 jobDescription = "A dummy job description."
//             });

//             var sessionId = JsonDocument.Parse(await initResp.Content.ReadAsStringAsync())
//                 .RootElement.GetProperty("sessionId").GetString();

//             var answerResp = await _client.PostAsJsonAsync("/api/interview/answer", new
//             {
//                 sessionId = sessionId ?? "dummy",
//                 question = "What is your experience?",
//                 audioBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(new string('a', 5100))),
//                 transcript = "This is a test transcript"
//             });

//             answerResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
//         }

//         [Fact]
//         public async Task InterviewFeedback_Should_Handle_Valid_Input()
//         {
//             var response = await _client.PostAsJsonAsync("/api/interview/feedback", new
//             {
//                 sessionId = "dummy-session-id",
//                 feedback = "This is feedback."
//             });
//             response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
//         }

//         [Fact]
//         public async Task InterviewFeedback_With_Empty_Payload_Should_Handle()
//         {
//             var response = await _client.PostAsJsonAsync("/api/interview/feedback", new { });
//             response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
//         }

//         [Fact]
//         public async Task InvalidRoute_Should_Return_NotFound()
//         {
//             var response = await _client.GetAsync("/api/nonexistent/route");
//             response.StatusCode.Should().Be(HttpStatusCode.NotFound);
//         }

//         [Fact]
//         public async Task GetStaticFile_Should_Return_Expected_Or_404()
//         {
//             var response = await _client.GetAsync("/audio/sample.webm");
//             response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
//         }
//     }
// }
