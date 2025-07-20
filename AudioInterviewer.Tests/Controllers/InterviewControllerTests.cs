using Xunit;
using Moq;
using AudioInterviewer.API.Controllers;
using AudioInterviewer.API.Services;
using AudioInterviewer.API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AudioInterviewer.Tests.Controllers
{
    public class InterviewControllerTests
    {
        private readonly Mock<IInterviewService> _serviceMock;
        private readonly InterviewController _controller;

        public InterviewControllerTests()
        {
            _serviceMock = new Mock<IInterviewService>();
            _controller = new InterviewController(_serviceMock.Object);
        }

        [Fact]
        public async Task InitializeInterview_ValidRequest_ReturnsOk()
        {
            var request = new InterviewController.InitRequest
            {
                Email = "test@example.com",
                JobDescription = "Looking for a backend developer"
            };
            _serviceMock.Setup(s => s.InitializeSessionAsync(It.IsAny<string>(), It.IsAny<string>()))
                        .ReturnsAsync("session123");
            _serviceMock.Setup(s => s.GetQuestions("session123"))
                        .Returns(new List<Question> { new Question { Text = "What is C#?" } });

            var result = await _controller.InitializeInterview(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            Assert.Contains("sessionId", okResult.Value!.ToString());
        }

        [Fact]
        public async Task GetNextQuestion_ValidSessionId_ReturnsQuestion()
        {
            _serviceMock.Setup(s => s.GetNextQuestionAsync("abc"))
                        .ReturnsAsync("What is your experience?");
            _serviceMock.Setup(s => s.CurrentIndex("abc"))
                        .Returns(1);

            var result = await _controller.GetNextQuestion("abc");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            Assert.Contains("question", okResult.Value!.ToString());
        }

        [Fact]
        public async Task SubmitAnswer_ValidPayload_ReturnsOk()
        {
            var dto = new AnswerDto
            {
                SessionId = "123",
                Question = "Explain DI",
                AudioBase64 = "audio",
                Transcript = "Dependency Injection"
            };

            _serviceMock.Setup(s => s.SubmitAnswerAsync(dto.SessionId, dto)).ReturnsAsync(true);
            _serviceMock.Setup(s => s.CurrentIndex(dto.SessionId)).Returns(2);

            var result = await _controller.SubmitAnswer(dto);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            Assert.Contains("Answer recorded", okResult.Value!.ToString());
        }

        [Fact]
        public async Task CompleteInterview_ValidSessionId_ReturnsSummary()
        {
            _serviceMock.Setup(s => s.GetCompletionSummaryAsync("123"))
                        .ReturnsAsync("Summary generated");

            var result = await _controller.CompleteInterview("123");
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Summary generated", okResult.Value);
        }

        [Fact]
        public async Task GetReport_ValidSessionId_ReturnsReport()
        {
            var report = new InterviewReport { Id = "abc", Email = "test@example.com" };
            _serviceMock.Setup(s => s.GenerateReportAsync("abc"))
                        .ReturnsAsync(report);

            var result = await _controller.GetReport("abc");
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(report, okResult.Value);
        }

        [Fact]
        public async Task GetReportsByEmail_ValidEmail_ReturnsReports()
        {
            var reports = new List<InterviewReport> {
                new InterviewReport { Id = "s1", Email = "test@example.com" },
                new InterviewReport { Id = "s2", Email = "test@example.com" }
            };

            _serviceMock.Setup(s => s.GetReportsByEmailAsync("test@example.com"))
                        .ReturnsAsync(reports);

            var result = await _controller.GetReportsByEmail("test@example.com");
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(reports, okResult.Value);
        }

        [Fact]
        public async Task GetReportById_ReportExists_ReturnsReport()
        {
            var report = new InterviewReport { Id = "id123", Email = "abc@test.com" };
            _serviceMock.Setup(s => s.GetReportByIdAsync("id123"))
                        .ReturnsAsync(report);

            var result = await _controller.GetReportById("id123");
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(report, okResult.Value);
        }

        [Fact]
        public async Task GetReportById_ReportMissing_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetReportByIdAsync("missing"))
                        .ReturnsAsync((InterviewReport?)null);

            var result = await _controller.GetReportById("missing");
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void HealthCheck_ReturnsHealthy()
        {
            var result = _controller.HealthCheck();
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Healthy", okResult.Value);
        }
    }
}
