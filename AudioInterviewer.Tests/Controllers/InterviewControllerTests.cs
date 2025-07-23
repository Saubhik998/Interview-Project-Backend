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
    public class InterviewControllerTests
    {
        private readonly Mock<IInterviewService> _mockService;
        private readonly InterviewController _controller;

        public InterviewControllerTests()
        {
            _mockService = new Mock<IInterviewService>();
            _controller = new InterviewController(_mockService.Object);
        }

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

            // Serialize and parse value as JsonElement for safe property access
            var json = JsonSerializer.Serialize(okResult.Value);
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

            Assert.Equal("Interview initialized", jsonElement.GetProperty("message").GetString());
            Assert.Equal("sessionId123", jsonElement.GetProperty("sessionId").GetString());
            Assert.Equal("First question?", jsonElement.GetProperty("firstQuestion").GetString());
        }

        [Fact]
        public async Task InitializeInterview_InvalidRequest_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("Email", "Invalid");
            var result = await _controller.InitializeInterview(new InterviewController.InitRequest());
            Assert.IsType<BadRequestObjectResult>(result);
        }

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

        [Fact]
        public async Task GetNextQuestion_MissingSessionId_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("SessionId", "Required");
            var result = await _controller.GetNextQuestion(null!);
            Assert.IsType<BadRequestObjectResult>(result);
        }

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

        [Fact]
        public async Task SubmitAnswer_InvalidPayload_ReturnsBadRequest()
        {
            var answer = new AnswerDto { SessionId = "", Question = "" };
            var result = await _controller.SubmitAnswer(answer);
            Assert.IsType<BadRequestObjectResult>(result);
        }

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

        [Fact]
        public async Task CompleteInterview_Valid_ReturnsSummary()
        {
            _mockService.Setup(s => s.GetCompletionSummaryAsync("session123")).ReturnsAsync("Completed!");

            var result = await _controller.CompleteInterview("session123");
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Completed!", okResult.Value);
        }

        [Fact]
        public async Task CompleteInterview_MissingSessionId_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("SessionId", "Required");
            var result = await _controller.CompleteInterview(null!);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetReport_Valid_ReturnsReport()
        {
            var report = new InterviewReport { Id = "r1", Email = "abc@test.com" };
            _mockService.Setup(s => s.GenerateReportAsync("session123")).ReturnsAsync(report);

            var result = await _controller.GetReport("session123");
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(report, okResult.Value);
        }

        [Fact]
        public async Task GetReport_MissingSessionId_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("SessionId", "Required");
            var result = await _controller.GetReport(null!);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetReportsByEmail_Valid_ReturnsReports()
        {
            var reports = new List<InterviewReport> { new InterviewReport { Id = "r1" } };
            _mockService.Setup(s => s.GetReportsByEmailAsync("user@t.com")).ReturnsAsync(reports);

            var result = await _controller.GetReportsByEmail("user@t.com");
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(reports, okResult.Value);
        }

        [Fact]
        public async Task GetReportsByEmail_MissingEmail_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("Email", "Required");
            var result = await _controller.GetReportsByEmail(null!);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetReportById_Valid_ReturnsReport()
        {
            var report = new InterviewReport { Id = "id1" };
            _mockService.Setup(s => s.GetReportByIdAsync("id1")).ReturnsAsync(report);

            var result = await _controller.GetReportById("id1");
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(report, okResult.Value);
        }

        [Fact]
        public async Task GetReportById_InvalidId_ReturnsBadRequest()
        {
            var result = await _controller.GetReportById(""); // Empty ID
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetReportById_NotFound_ReturnsNotFound()
        {
            _mockService.Setup(s => s.GetReportByIdAsync("unknown")).ReturnsAsync((InterviewReport)null);

            var result = await _controller.GetReportById("unknown");
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void HealthCheck_ReturnsOk()
        {
            var result = _controller.HealthCheck();
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Healthy", okResult.Value);
        }
    }
}
