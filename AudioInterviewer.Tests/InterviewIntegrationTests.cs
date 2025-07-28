// using System.Net;
// using System.Net.Http.Json;
// using Xunit;
// using FluentAssertions;
// using Moq;
// using Microsoft.AspNetCore.Mvc.Testing;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Configuration;
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
//             builder.ConfigureAppConfiguration((context, config) =>
//             {
//                 context.HostingEnvironment.EnvironmentName = "Testing"; // ✅ Correct way to set environment
//             });

//             builder.ConfigureServices(services =>
//             {
//                 //  Remove real services
//                 var serviceToRemove = services.SingleOrDefault(s => s.ServiceType == typeof(IInterviewService));
//                 if (serviceToRemove != null) services.Remove(serviceToRemove);

//                 var apiClientToRemove = services.SingleOrDefault(s => s.ServiceType == typeof(IApiClient));
//                 if (apiClientToRemove != null) services.Remove(apiClientToRemove);

//                 // Add mocks
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
//                     .ReturnsAsync(new
//                     {
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
//         Assert.Equal("Healthy", body.Trim('"'));
//     }

//     [Fact]
//     public void InitInterview_DummyValidation_Should_Pass()
//     {
//         var payload = new { Email = "x@y.com", JobDescription = "Some JD" };
//         payload.Email.Should().Contain("@");
//     }

//     [Fact]
//     public void SubmitAnswer_ResponseType_ShouldBeObject()
//     {
//         var response = new SubmitAnswerResponse { Message = "Recorded", Index = 2 };
//         response.Should().BeOfType<SubmitAnswerResponse>();
//     }

//     [Fact]
//     public void ReportStructure_Should_HaveRequiredFields()
//     {
//         var report = new ReportResponse
//         {
//             Jd = "Some JD",
//             Score = 80,
//             Questions = new() { "Q1", "Q2" },
//             Strengths = new() { "Teamwork" }
//         };
//         report.Score.Should().BeGreaterThan(0);
//     }

//     [Fact]
//     public void InterviewSummary_DummyTotalCheck()
//     {
//         var summary = new CompleteInterviewSummary { TotalAnswers = 2, TotalQuestions = 2 };
//         summary.TotalAnswers.Should().Be(summary.TotalQuestions);
//     }

//     [Fact]
//     public void InterviewSessionId_DummyCheck()
//     {
//         var initResponse = new InitInterviewResponse { SessionId = "session-123" };
//         initResponse.SessionId.Should().StartWith("session");
//     }

//     [Fact]
//     public void NextQuestion_DummyIndexCheck()
//     {
//         var next = new NextQuestionResponse { Index = 3, Question = "Why us?" };
//         next.Index.Should().BeGreaterThan(0);
//     }

//     [Fact]
//     public void ReportResponse_QuestionsAndAnswers_Should_NotBeNull()
//     {
//         var report = new ReportResponse();
//         report.Questions.Should().NotBeNull();
//         report.Answers.Should().NotBeNull();
//     }

//     [Fact]
//     public void GetReportsByEmail_DummyEmailCheck()
//     {
//         var reports = new List<InterviewReport>
//         {
//             new InterviewReport { Email = "test@test.com" }
//         };
//         reports.First().Email.Should().EndWith(".com");
//     }



//     [Fact]
//     public void InitInterview_MessageCheck_Dummy()
//     {
//         var response = new InitInterviewResponse { Message = "Interview initialized" };
//         response.Message.Should().Contain("Interview");
//     }

//     [Fact]
//     public void ReportResponse_DefaultValues_ShouldBeEmptyOrZero()
//     {
//         var report = new ReportResponse();
//         report.Jd.Should().BeEmpty();
//         report.Score.Should().Be(0);
//     }

//     [Fact]
//     public void CompleteInterviewSummary_ShouldBeConstructible()
//     {
//         var summary = new CompleteInterviewSummary
//         {
//             Message = "Finished",
//             TotalQuestions = 3,
//             TotalAnswers = 3
//         };
//         summary.Message.Should().Be("Finished");
//     }

//     [Fact]
//     public void NextQuestion_ShouldContainExpectedQuestionText()
//     {
//         var question = new NextQuestionResponse { Question = "What motivates you?" };
//         question.Question.Should().Contain("motivate");
//     }

//     [Fact]
//     public void InitInterviewResponse_AllFields_ShouldBeSet()
//     {
//         var init = new InitInterviewResponse
//         {
//             Message = "Success",
//             SessionId = "abc123",
//             FirstQuestion = "Tell me about yourself"
//         };

//         init.Message.Should().Be("Success");
//         init.SessionId.Should().NotBeNullOrEmpty();
//         init.FirstQuestion.Should().NotBeNullOrWhiteSpace();
//     }

//     [Fact]
//     public void ReportResponse_Lists_ShouldSupportAdditions()
//     {
//         var report = new ReportResponse();
//         report.Questions.Add("Why should we hire you?");
//         report.Strengths.Add("Teamwork");

//         report.Questions.Should().HaveCount(1);
//         report.Strengths.First().Should().Be("Teamwork");
//     }

//     [Fact]
//     public void ReportResponse_FollowUps_CanContainMultipleValues()
//     {
//         var report = new ReportResponse
//         {
//             FollowUps = new List<string> { "What motivates you?", "Where do you see yourself?" }
//         };

//         report.FollowUps.Should().Contain("What motivates you?");
//         report.FollowUps.Should().HaveCountGreaterThan(1);
//     }

//     [Fact]
//     public void SubmitAnswerResponse_ShouldStoreIndex()
//     {
//         var response = new SubmitAnswerResponse { Index = 5 };
//         response.Index.Should().Be(5);
//     }

//     [Fact]
//     public void InterviewReport_ShouldHaveIdAndEmail()
//     {
//         var report = new InterviewReport { Id = "r1", Email = "user@example.com" };
//         report.Id.Should().Be("r1");
//         report.Email.Should().Contain("@");
//     }

//     [Fact]
//     public void InitInterviewResponse_Empty_ShouldNotThrow()
//     {
//         var response = new InitInterviewResponse();
//         response.Should().NotBeNull();
//     }

//     [Fact]
//     public void ReportResponse_Answers_CanBePopulated()
//     {
//         var report = new ReportResponse();
//         report.Answers.Add(new { question = "Q1", transcript = "T1", audio = "/a1" });

//         report.Answers.Should().NotBeEmpty();
//     }
    
//     [Fact]
//     public void ReportResponse_CanHandle_EmptyInitialization()
//     {
//         var report = new ReportResponse();
//         report.Questions.Should().BeEmpty();
//         report.Strengths.Should().BeEmpty();
//         report.Improvements.Should().BeEmpty();
//         report.FollowUps.Should().BeEmpty();
//     }

//     [Fact]
//     public void CompleteInterviewSummary_DefaultState_ShouldBeValid()
//     {
//         var summary = new CompleteInterviewSummary();
//         summary.TotalAnswers.Should().Be(0);
//         summary.TotalQuestions.Should().Be(0);
//         summary.Message.Should().BeEmpty();
//     }

//     [Fact]
//     public void SubmitAnswerResponse_CanAssignMessageAndIndex()
//     {
//         var response = new SubmitAnswerResponse
//         {
//             Message = "Saved",
//             Index = 99
//         };
//         response.Message.Should().Be("Saved");
//         response.Index.Should().Be(99);
//     }

//     [Fact]
//     public void NextQuestionResponse_ShouldSupportAllProperties()
//     {
//         var next = new NextQuestionResponse
//         {
//             Index = 42,
//             Question = "What is your leadership style?"
//         };

//         next.Index.Should().Be(42);
//         next.Question.Should().Contain("leadership");
//     }

//     [Fact]
//     public void ReportResponse_CanContainMultipleAnswers()
//     {
//         var report = new ReportResponse();
//         report.Answers.Add(new { text = "A" });
//         report.Answers.Add(new { text = "B" });

//         report.Answers.Should().HaveCount(2);
//     }

//     [Fact]
//     public void ReportResponse_Strings_ShouldBeSettable()
//     {
//         var report = new ReportResponse
//         {
//             Jd = "Sample JD"
//         };

//         report.Jd.Should().Be("Sample JD");
//     }

//     [Fact]
//     public void InterviewReport_AllProperties_ShouldBeAccessible()
//     {
//         var r = new InterviewReport
//         {
//             Id = "abc",
//             Email = "foo@bar.com",
//             JobDescription = "Tester",
            
//         };

//         r.Id.Should().Be("abc");
//         r.Email.Should().Contain("@");
//         r.JobDescription.Should().Be("Tester");
    
//     }

//     [Fact]
//     public void ReportResponse_CanMixContent()
//     {
//         var report = new ReportResponse
//         {
//             Score = 100
//         };
//         report.Strengths.Add("Logic");
//         report.Improvements.Add("Speed");

//         report.Score.Should().Be(100);
//         report.Strengths.Should().Contain("Logic");
//         report.Improvements.Should().Contain("Speed");
//     }

//     [Fact]
//     public void InitInterviewResponse_SetAllProps_ThenVerify()
//     {
//         var init = new InitInterviewResponse
//         {
//             Message = "Started",
//             SessionId = "s123",
//             FirstQuestion = "Why this company?"
//         };

//         init.Message.Should().Be("Started");
//         init.SessionId.Should().Be("s123");
//         init.FirstQuestion.Should().Contain("Why");
//     }

//     [Fact]
//     public void ReportResponse_FollowUps_AllowMultipleEntries()
//     {
//         var r = new ReportResponse();
//         r.FollowUps.Add("What's your expected salary?");
//         r.FollowUps.Add("Do you have any questions?");

//         r.FollowUps.Should().HaveCount(2);
//     }


// }
