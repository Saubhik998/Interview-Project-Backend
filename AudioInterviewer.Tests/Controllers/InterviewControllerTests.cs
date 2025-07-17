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
    /// <summary>
    /// Unit tests for the <see cref="InterviewController"/> class.
    /// </summary>
    public class InterviewControllerTests
    {
        private readonly Mock<IInterviewService> _mockService;
        private readonly InterviewController _controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewControllerTests"/> class.
        /// </summary>
        public InterviewControllerTests()
        {
            _mockService = new Mock<IInterviewService>();
            _controller = new InterviewController(_mockService.Object);
        }

        /// <summary>
        /// Tests that InitializeInterview returns 200 OK with the first question.
        /// </summary>
        [Fact]
        public async Task InitializeInterview_ReturnsOk_WithFirstQuestion()
        {
            // Arrange
            var request = new InterviewController.InitRequest
            {
                Email = "test@example.com",
                JobDescription = "Backend Developer"
            };

            _mockService.Setup(s => s.InitializeSessionAsync(request.JobDescription, request.Email))
                .Returns(Task.CompletedTask);
            _mockService.Setup(s => s.GetQuestions())
                .Returns(new List<Question> { new Question { Text = "What is dependency injection?" } });

            // Act
            var result = await _controller.InitializeInterview(request) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Value);
        }

        /// <summary>
        /// Tests that GetNextQuestion returns 200 OK with the next question.
        /// </summary>
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
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Value);
        }

        /// <summary>
        /// Tests that SubmitAnswer returns 200 OK when the answer is successfully recorded.
        /// </summary>
        [Fact]
        public async Task SubmitAnswer_ReturnsOk_WhenAnswerIsAccepted()
        {
            // Arrange
            var answerDto = new AnswerDto
            {
                Question = "What is REST?",
                AudioBase64 = "fakeaudio==",
                Transcript = "REST is an architectural style..."
            };

            _mockService.Setup(s => s.SubmitAnswerAsync(answerDto)).ReturnsAsync(true);
            _mockService.Setup(s => s.CurrentIndex).Returns(2);

            // Act
            var result = await _controller.SubmitAnswer(answerDto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Value);
        }

        /// <summary>
        /// Tests that CompleteInterview returns summary with 200 OK status.
        /// </summary>
        [Fact]
        public async Task CompleteInterview_ReturnsOk_WithSummary()
        {
            // Arrange
            var expectedSummary = new
            {
                message = "Interview completed",
                totalQuestions = 5,
                totalAnswers = 5
            };

            _mockService.Setup(s => s.GetCompletionSummaryAsync()).ReturnsAsync(expectedSummary);

            // Act
            var result = await _controller.CompleteInterview() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(expectedSummary, result.Value);
        }

        /// <summary>
        /// Tests that GetReport returns 200 OK with a mock report.
        /// </summary>
        [Fact]
        public async Task GetReport_ReturnsOk_WithReport()
        {
            // Arrange
            var report = new
            {
                jd = "Backend Developer",
                score = 90,
                questions = new List<string> { "Tell me about async programming." },
                answers = new List<object>(),
                strengths = new List<string> { "Analytical thinking" },
                improvements = new List<string> { "Clarify answers" },
                followUps = new List<string> { "Ask about microservices" }
            };

            _mockService.Setup(s => s.GenerateReportAsync()).ReturnsAsync(report);

            // Act
            var result = await _controller.GetReport() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(report, result.Value);
        }

        /// <summary>
        /// Tests that GetReportsByEmail returns a list of reports for a valid email.
        /// </summary>
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

        /// <summary>
        /// Tests that GetReportById returns a report if it exists.
        /// </summary>
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

        /// <summary>
        /// Tests that GetReportById returns 404 NotFound when the report does not exist.
        /// </summary>
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

        /// <summary>
        /// Tests that InitializeInterview returns 400 BadRequest if model validation fails.
        /// </summary>
        [Fact]
        public async Task InitializeInterview_ReturnsBadRequest_WhenModelInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("JobDescription", "Required");

            var request = new InterviewController.InitRequest
            {
                Email = "test@example.com",
                JobDescription = ""
            };

            // Act
            var result = await _controller.InitializeInterview(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
