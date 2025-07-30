using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using AudioInterviewer.API.Controllers;
using AudioInterviewer.API.Models;
using AudioInterviewer.API.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using System;

namespace AudioInterviewer.UnitTests.Controllers
{
    /// <summary>
    /// Contains unit tests for the <see cref="InterviewController"/> API endpoints,
    /// validating their behavior in response to various scenarios.
    /// </summary>
    public class InterviewControllerTests
    {
        private readonly Mock<IInterviewService> _mockService;
        private readonly InterviewController _controller;

        /// <summary>
        /// Initializes a new instance of <see cref="InterviewControllerTests"/>, setting up mocks and controller.
        /// </summary>
        public InterviewControllerTests()
        {
            _mockService = new Mock<IInterviewService>();
            _controller = new InterviewController(_mockService.Object);
        }

        /// <summary>
        /// Tests that <see cref="InterviewController.InitializeInterview"/> returns <see cref="OkObjectResult"/>
        /// with correct payload when the request is valid.
        /// </summary>
        [Fact]
        public async Task InitializeInterview_ValidRequest_ReturnsOk()
        {
            var request = new InterviewController.InitRequest
            {
                Email = "user@site.com",
                JobDescription = "Software developer role."
            };

            _mockService.Setup(s => s.InitializeSessionAsync(request.JobDescription, request.Email))
                .ReturnsAsync("sessionId123");
            _mockService.Setup(s => s.GetQuestions("sessionId123"))
                .Returns(new List<Question> { new Question { Text = "First question?" } });

            var result = await _controller.InitializeInterview(request);

            var okResult = Assert.IsType<OkObjectResult>(result);

            var json = JsonSerializer.Serialize(okResult.Value);
            // Optionally log the JSON to debug output if needed
            // Console.WriteLine("Response JSON: " + json);

            var responseObj = JsonSerializer.Deserialize<InitResponse>(json);

            Assert.NotNull(responseObj);
            Assert.Equal("Interview initialized", responseObj.Message);
            Assert.Equal("sessionId123", responseObj.SessionId);
            Assert.Equal("First question?", responseObj.FirstQuestion);
        }

        /// <summary>
        /// Tests that <see cref="InterviewController.InitializeInterview"/> returns <see cref="BadRequestObjectResult"/>
        /// when the request model state is invalid.
        /// </summary>
        [Fact]
        public async Task InitializeInterview_InvalidRequest_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("Email", "Invalid");

            var result = await _controller.InitializeInterview(new InterviewController.InitRequest());
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that <see cref="InterviewController.GetNextQuestion"/> returns the next question and index given a valid session.
        /// </summary>
        [Fact]
        public async Task GetNextQuestion_ValidSession_ReturnsNextQuestion()
        {
            _mockService.Setup(s => s.GetNextQuestionAsync("session123")).ReturnsAsync("What is your strength?");
            _mockService.Setup(s => s.CurrentIndex("session123")).Returns(2);

            var result = await _controller.GetNextQuestion("session123");

            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(okResult.Value);
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

            Assert.Equal(2, jsonElement.GetProperty("index").GetInt32());
            Assert.Equal("What is your strength?", jsonElement.GetProperty("question").GetString());
        }

        /// <summary>
        /// Tests that <see cref="InterviewController.GetNextQuestion"/> returns a completion message if there are no more questions.
        /// </summary>
        [Fact]
        public async Task GetNextQuestion_Complete_ReturnsCompleteMessage()
        {
            _mockService.Setup(s => s.GetNextQuestionAsync("session123")).ReturnsAsync(string.Empty);

            var result = await _controller.GetNextQuestion("session123");

            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(okResult.Value);
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

            Assert.Equal("Interview complete", jsonElement.GetProperty("message").GetString());
        }

        /// <summary>
        /// Tests that <see cref="InterviewController.GetNextQuestion"/> returns <see cref="BadRequestObjectResult"/>
        /// when sessionId parameter is missing or model state is invalid.
        /// </summary>
        [Fact]
        public async Task GetNextQuestion_MissingSessionId_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("SessionId", "Required");
            var result = await _controller.GetNextQuestion(null!);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that <see cref="InterviewController.SubmitAnswer"/> returns <see cref="OkObjectResult"/>
        /// and the correct index when a valid answer is submitted.
        /// </summary>
        [Fact]
        public async Task SubmitAnswer_ValidAnswer_ReturnsOk()
        {
            var answer = new AnswerDto
            {
                SessionId = "sess001",
                Question = "Describe your experience.",
                AudioBase64 = new string('a', 5000),
                Transcript = "Sample transcript."
            };

            _mockService.Setup(s => s.SubmitAnswerAsync("sess001", answer)).ReturnsAsync(true);
            _mockService.Setup(s => s.CurrentIndex("sess001")).Returns(2);

            var result = await _controller.SubmitAnswer(answer);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(okResult.Value);
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

            Assert.Equal("Answer recorded", jsonElement.GetProperty("message").GetString());
            Assert.Equal(2, jsonElement.GetProperty("index").GetInt32());
        }

        /// <summary>
        /// Tests that <see cref="InterviewController.SubmitAnswer"/> returns <see cref="BadRequestObjectResult"/>
        /// when the answer payload is invalid.
        /// </summary>
        [Fact]
        public async Task SubmitAnswer_InvalidPayload_ReturnsBadRequest()
        {
            var answer = new AnswerDto { SessionId = "", Question = "" };
            var result = await _controller.SubmitAnswer(answer);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that <see cref="InterviewController.SubmitAnswer"/> returns <see cref="BadRequestObjectResult"/>
        /// if the service reports no more questions.
        /// </summary>
        [Fact]
        public async Task SubmitAnswer_ServiceReturnsFalse_ReturnsBadRequest()
        {
            var answer = new AnswerDto
            {
                SessionId = "sess001",
                Question = "Describe your experience.",
                AudioBase64 = "validaudio",
                Transcript = "transcript"
            };

            _mockService.Setup(s => s.SubmitAnswerAsync("sess001", answer)).ReturnsAsync(false);

            var result = await _controller.SubmitAnswer(answer);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that <see cref="InterviewController.CompleteInterview"/> returns the expected summary object on completion.
        /// </summary>
        [Fact]
        public async Task CompleteInterview_Valid_ReturnsSummary()
        {
            _mockService.Setup(s => s.GetCompletionSummaryAsync("session123")).ReturnsAsync("Completed!");

            var result = await _controller.CompleteInterview("session123");
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Completed!", okResult.Value);
        }

        /// <summary>
        /// Tests that <see cref="InterviewController.CompleteInterview"/> returns <see cref="BadRequestObjectResult"/>
        /// when sessionId is missing or model state is invalid.
        /// </summary>
        [Fact]
        public async Task CompleteInterview_MissingSessionId_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("SessionId", "Required");
            var result = await _controller.CompleteInterview(null!);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that <see cref="InterviewController.GetReport"/> returns the correct report for a valid session.
        /// </summary>
        [Fact]
        public async Task GetReport_Valid_ReturnsReport()
        {
            var report = new InterviewReport { Id = "r1", Email = "abc@test.com" };
            _mockService.Setup(s => s.GenerateReportAsync("session123")).ReturnsAsync(report);

            var result = await _controller.GetReport("session123");
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(report, okResult.Value);
        }

        /// <summary>
        /// Tests that <see cref="InterviewController.GetReport"/> returns <see cref="BadRequestObjectResult"/>
        /// when sessionId is missing or model state is invalid.
        /// </summary>
        [Fact]
        public async Task GetReport_MissingSessionId_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("SessionId", "Required");
            var result = await _controller.GetReport(null!);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that <see cref="InterviewController.GetReportsByEmail"/> returns all reports for the provided email.
        /// </summary>
        [Fact]
        public async Task GetReportsByEmail_Valid_ReturnsReports()
        {
            var reports = new List<InterviewReport> { new InterviewReport { Id = "r1" } };
            _mockService.Setup(s => s.GetReportsByEmailAsync("user@t.com")).ReturnsAsync(reports);

            var result = await _controller.GetReportsByEmail("user@t.com");
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(reports, okResult.Value);
        }

        /// <summary>
        /// Tests that <see cref="InterviewController.GetReportsByEmail"/> returns <see cref="BadRequestObjectResult"/>
        /// when email is missing or model state is invalid.
        /// </summary>
        [Fact]
        public async Task GetReportsByEmail_MissingEmail_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("Email", "Required");
            var result = await _controller.GetReportsByEmail(null!);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that <see cref="InterviewController.GetReportById"/> returns the expected report when found by ID.
        /// </summary>
        [Fact]
        public async Task GetReportById_Valid_ReturnsReport()
        {
            var report = new InterviewReport { Id = "id1" };
            _mockService.Setup(s => s.GetReportByIdAsync("id1")).ReturnsAsync(report);

            var result = await _controller.GetReportById("id1");
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(report, okResult.Value);
        }

        /// <summary>
        /// Tests that <see cref="InterviewController.GetReportById"/> returns <see cref="BadRequestObjectResult"/>
        /// if the report ID is missing or empty.
        /// </summary>
        [Fact]
        public async Task GetReportById_InvalidId_ReturnsBadRequest()
        {
            var result = await _controller.GetReportById(""); // Empty ID
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that <see cref="InterviewController.GetReportById"/> returns <see cref="NotFoundObjectResult"/>
        /// when the report is not found.
        /// </summary>
        [Fact]
        public async Task GetReportById_NotFound_ReturnsNotFound()
        {
            _mockService.Setup(s => s.GetReportByIdAsync("unknown")).ReturnsAsync((InterviewReport)null);

            var result = await _controller.GetReportById("unknown");
            Assert.IsType<NotFoundObjectResult>(result);
        }

        /// <summary>
        /// Tests that <see cref="InterviewController.HealthCheck"/> returns <see cref="OkObjectResult"/>
        /// with the value "Healthy".
        /// </summary>
        [Fact]
        public void HealthCheck_ReturnsOk()
        {
            var result = _controller.HealthCheck();
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Healthy", okResult.Value);
        }
    }
}
