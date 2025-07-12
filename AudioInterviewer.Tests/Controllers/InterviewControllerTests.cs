using AudioInterviewer.API.Controllers;
using AudioInterviewer.API.Models;
using AudioInterviewer.API.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace AudioInterviewer.Tests.Controllers
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
        public async Task InitializeInterview_ReturnsOk_WithFirstQuestion()
        {
            // Arrange
            var request = new InterviewController.InitRequest
            {
                Email = "test@example.com",
                JobDescription = "Backend Developer"
            };

            _mockService.Setup(s => s.InitializeSessionAsync(request.JobDescription, request.Email)).Returns(Task.CompletedTask);
            _mockService.Setup(s => s.GetQuestions()).Returns(new List<Question>
            {
                new Question { Text = "What is dependency injection?" }
            });

            // Act
            var result = await _controller.InitializeInterview(request) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Value);
        }

        [Fact]
        public async Task GetNextQuestion_ReturnsOk_WithNextQuestion()
        {
            // Arrange
            _mockService.Setup(s => s.GetNextQuestionAsync()).ReturnsAsync("Explain polymorphism.");
            _mockService.Setup(s => s.CurrentIndex).Returns(1);

            // Act
            var result = await _controller.GetNextQuestion() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result?.StatusCode);
            Assert.NotNull(result?.Value);
        }

        [Fact]
        public void SubmitAnswer_ReturnsOk_WhenAnswerIsAccepted()
        {
            // Arrange
            var answerDto = new AnswerDto
            {
                Question = "What is REST?",
                AudioBase64 = "fakeaudio==",
                Transcript = "REST is an architectural style..."
            };

            _mockService.Setup(s => s.SubmitAnswer(answerDto)).Returns(true);
            _mockService.Setup(s => s.CurrentIndex).Returns(2);

            // Act
            var result = _controller.SubmitAnswer(answerDto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Value);
        }

        [Fact]
        public void CompleteInterview_ReturnsOk_WithSummary()
        {
            // Arrange
            var expectedSummary = new
            {
                CandidateFitScore = 85,
                Strengths = new List<string> { "Strong fundamentals", "Clear communication" },
                ImprovementAreas = new List<string> { "More examples", "Expand technical details" },
                SuggestedFollowUp = "Ask about database scaling"
            };

            _mockService.Setup(s => s.GetCompletionSummary()).Returns(expectedSummary);

            // Act
            var result = _controller.CompleteInterview() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Value);
        }

        [Fact]
        public async Task GetReport_ReturnsOk_WithReport()
        {
            // Arrange
            var report = new InterviewReport
            {
                Email = "test@example.com",
                JobDescription = "Backend Developer",
                CandidateFitScore = 90,
                Strengths = new List<string> { "Analytical thinking" },
                ImprovementAreas = new List<string> { "Clarify answers" },
                SuggestedFollowUp = { "Ask about microservices" },
                Answers = new List<Answer>()
            };

            _mockService.Setup(s => s.GenerateReportAsync()).ReturnsAsync(report);

            // Act
            var result = await _controller.GetReport() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(report, result.Value);
        }

        [Fact]
        public async Task GetReportsByEmail_ReturnsOk_WithReportsList()
        {
            // Arrange
            var email = "user@example.com";
            var reports = new List<InterviewReport>
            {
                new InterviewReport { Email = email },
                new InterviewReport { Email = email }
            };

            _mockService.Setup(s => s.GetReportsByEmailAsync(email)).ReturnsAsync(reports);

            // Act
            var result = await _controller.GetReportsByEmail(email) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(reports, result.Value);
        }

        [Fact]
        public async Task GetReportById_ReturnsOk_WhenReportExists()
        {
            // Arrange
            var reportId = "123";
            var report = new InterviewReport { Email = "test@example.com" };

            _mockService.Setup(s => s.GetReportByIdAsync(reportId)).ReturnsAsync(report);

            // Act
            var result = await _controller.GetReportById(reportId) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(report, result.Value);
        }

        [Fact]
        public async Task GetReportById_ReturnsNotFound_WhenReportDoesNotExist()
        {
            // Arrange
            var reportId = "invalid-id";
            _mockService.Setup(s => s.GetReportByIdAsync(reportId)).ReturnsAsync((InterviewReport)null);

            // Act
            var result = await _controller.GetReportById(reportId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
