// using System.Net;
// using System.Net.Http.Json;
// using Xunit;
// using FluentAssertions;
// using Moq;
// using Microsoft.AspNetCore.Mvc.Testing;
// using Microsoft.Extensions.DependencyInjection;
// using System.Linq;
// using System.Collections.Generic;
// using AudioInterviewer.API.Models;
// using AudioInterviewer.API.Services;
// using AudioInterviewer.API.Services.External;

// // ===================== DTOs matching response shapes ======================

// public class InitInterviewResponse
// {
//     public string Message { get; set; } = "";
//     public string SessionId { get; set; } = "";
//     public string FirstQuestion { get; set; } = "";
// }

// public class NextQuestionResponse
// {
//     public int Index { get; set; }
//     public string Question { get; set; } = "";
// }

// public class SubmitAnswerResponse
// {
//     public string Message { get; set; } = "";
//     public int Index { get; set; }
// }

// public class CompleteInterviewSummary
// {
//     public string Message { get; set; } = "";
//     public int TotalQuestions { get; set; }
//     public int TotalAnswers { get; set; }
// }

// public class ReportResponse
// {
//     public string Jd { get; set; } = "";
//     public int Score { get; set; }
//     public List<string> Questions { get; set; } = new();
//     public List<object> Answers { get; set; } = new();
//     public List<string> Strengths { get; set; } = new();
//     public List<string> Improvements { get; set; } = new();
//     public List<string> FollowUps { get; set; } = new();
// }

// // ==========================================================================

// public class InterviewControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
// {
//     private readonly WebApplicationFactory<Program> _factory;
//     private readonly HttpClient _client;

//     public InterviewControllerIntegrationTests(WebApplicationFactory<Program> factory)
//     {
//         _factory = factory.WithWebHostBuilder(builder =>
//         {
//             builder.ConfigureServices(services =>
//             {
//                 var interviewService = new Mock<IInterviewService>();
//                 var fastApiClient = new Mock<IApiClient>();

//                 interviewService
//                     .Setup(s => s.InitializeSessionAsync(It.IsAny<string>(), It.IsAny<string>()))
//                     .ReturnsAsync("test-session-id");

//                 interviewService
//                     .Setup(s => s.GetQuestions("test-session-id"))
//                     .Returns(new List<Question> { new Question { Text = "What is your strength?" } });

//                 interviewService
//                     .Setup(s => s.GetNextQuestionAsync("test-session-id"))
//                     .ReturnsAsync("What is your biggest weakness?");

//                 interviewService
//                     .Setup(s => s.SubmitAnswerAsync(It.IsAny<string>(), It.IsAny<AnswerDto>()))
//                     .ReturnsAsync(true);

//                 interviewService
//                     .Setup(s => s.CurrentIndex("test-session-id"))
//                     .Returns(1);

//                 interviewService
//                     .Setup(s => s.GetCompletionSummaryAsync("test-session-id"))
//                     .ReturnsAsync(new { message = "Interview completed", totalQuestions = 2, totalAnswers = 2 });

//                 interviewService
//                     .Setup(s => s.GenerateReportAsync("test-session-id"))
//                     .ReturnsAsync(new { 
//                         jd = "A test job description for QA automation", 
//                         score = 95,
//                         questions = new List<string> { "What is your strength?", "What is your biggest weakness?" },
//                         answers = new List<object> { 
//                             new { question = "What is your strength?", transcript = "I am detail-oriented.", audio = "/audio/1" } 
//                         },
//                         strengths = new[] { "Communication" }, 
//                         improvements = new[] { "Detail orientation" },
//                         followUps = new[] { "What motivates you?" }
//                     });

//                 interviewService
//                     .Setup(s => s.GetReportsByEmailAsync(It.IsAny<string>()))
//                     .ReturnsAsync(new List<InterviewReport> { 
//                         new InterviewReport { Id = "report-id", Email = "test@test.com", JobDescription = "Developer" } 
//                     });

//                 interviewService
//                     .Setup(s => s.GetReportByIdAsync("report-id"))
//                     .ReturnsAsync(new InterviewReport { Id = "report-id", Email = "test@test.com", JobDescription = "Developer" });

//                 services.AddSingleton(interviewService.Object);
//                 services.AddSingleton(fastApiClient.Object);
//             });
//         });

//         _client = _factory.CreateClient();
//     }

//     [Fact]
//     public async Task POST_InitInterview_ShouldReturnSessionIdAndFirstQuestion()
//     {
//         var payload = new
//         {
//             Email = "test@test.com",
//             JobDescription = "A test job description for QA automation"
//         };

//         var response = await _client.PostAsJsonAsync("/api/interview/init", payload);
//         response.StatusCode.Should().Be(HttpStatusCode.OK);

//         var result = await response.Content.ReadFromJsonAsync<InitInterviewResponse>();
//         Assert.Equal("Interview initialized", result.Message);
//         Assert.Equal("test-session-id", result.SessionId);
//         Assert.Equal("What is your strength?", result.FirstQuestion);
//     }

//     [Fact]
//     public async Task GET_GetNextQuestion_ShouldReturnNextQuestion()
//     {
//         var response = await _client.GetAsync("/api/interview/question?sessionId=test-session-id");
//         response.StatusCode.Should().Be(HttpStatusCode.OK);

//         var result = await response.Content.ReadFromJsonAsync<NextQuestionResponse>();
//         Assert.Equal(1, result.Index);
//         Assert.Equal("What is your biggest weakness?", result.Question);
//     }

//     [Fact]
//     public async Task POST_SubmitAnswer_ShouldSucceed()
//     {
//         var answer = new
//         {
//             SessionId = "test-session-id",
//             Question = "What is your strength?",
//             AudioBase64 = Convert.ToBase64String(Enumerable.Repeat((byte)0, 6000).ToArray()),
//             Transcript = "My strength is attention to detail."
//         };

//         var response = await _client.PostAsJsonAsync("/api/interview/answer", answer);
//         response.StatusCode.Should().Be(HttpStatusCode.OK);

//         var result = await response.Content.ReadFromJsonAsync<SubmitAnswerResponse>();
//         Assert.Equal("Answer recorded", result.Message);
//         Assert.Equal(1, result.Index);
//     }

//     [Fact]
//     public async Task POST_CompleteInterview_ShouldReturnSummary()
//     {
//         var response = await _client.PostAsync("/api/interview/complete?sessionId=test-session-id", null);
//         response.StatusCode.Should().Be(HttpStatusCode.OK);

//         var result = await response.Content.ReadFromJsonAsync<CompleteInterviewSummary>();
//         Assert.Equal("Interview completed", result.Message);
//         Assert.Equal(2, result.TotalQuestions);
//         Assert.Equal(2, result.TotalAnswers);
//     }

//     [Fact]
//     public async Task GET_GetReport_ShouldReturnReport()
//     {
//         var response = await _client.GetAsync("/api/interview/report?sessionId=test-session-id");
//         response.StatusCode.Should().Be(HttpStatusCode.OK);

//         var result = await response.Content.ReadFromJsonAsync<ReportResponse>();
//         Assert.Equal(95, result.Score);
//         Assert.Contains("Communication", result.Strengths);
//     }

//     [Fact]
//     public async Task GET_GetReportsByEmail_ShouldReturnReports()
//     {
//         var response = await _client.GetAsync("/api/interview/reports?email=test@test.com");
//         response.StatusCode.Should().Be(HttpStatusCode.OK);

//         var reports = await response.Content.ReadFromJsonAsync<InterviewReport[]>();
//         Assert.Single(reports);
//         Assert.Equal("test@test.com", reports[0].Email);
//     }

//     [Fact]
//     public async Task GET_GetReportById_ShouldReturnSpecificReport()
//     {
//         var response = await _client.GetAsync("/api/interview/report/report-id");
//         response.StatusCode.Should().Be(HttpStatusCode.OK);

//         var report = await response.Content.ReadFromJsonAsync<InterviewReport>();
//         Assert.Equal("Developer", report.JobDescription);
//     }

//     [Fact]
//     public async Task GET_HealthCheck_ReturnsSuccess()
//     {
//         var response = await _client.GetAsync("/api/interview/health");
//         response.StatusCode.Should().Be(HttpStatusCode.OK);

//         var body = await response.Content.ReadAsStringAsync();
//         Assert.Equal("Healthy", body.Trim('"')); // Might be JSON "Healthy"
//     }
// }
// // 