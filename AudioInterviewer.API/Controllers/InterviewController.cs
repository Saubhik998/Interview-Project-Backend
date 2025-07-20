using Microsoft.AspNetCore.Mvc;
using AudioInterviewer.API.Models;
using AudioInterviewer.API.Services;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AudioInterviewer.API.Controllers
{
    /// <summary>
    /// Handles all endpoints related to the interview process such as initialization, questions, answers, and reports.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class InterviewController : ControllerBase
    {
        private readonly IInterviewService _interviewService;

        /// <summary>
        /// Constructor that injects the interview service.
        /// </summary>
        public InterviewController(IInterviewService interviewService)
        {
            _interviewService = interviewService;
        }

        /// <summary>
        /// Request payload for initializing an interview.
        /// </summary>
        public class InitRequest
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [MinLength(10, ErrorMessage = "Job description must be at least 10 characters long.")]
            public string JobDescription { get; set; } = string.Empty;
        }

        /// <summary>
        /// Initializes a new interview session with the given email and job description.
        /// </summary>
        [HttpPost("init")]
        public async Task<IActionResult> InitializeInterview([FromBody] InitRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { error = "Invalid input", details = ModelState });

            // service returns a new sessionId
            var sessionId = await _interviewService.InitializeSessionAsync(request.JobDescription, request.Email);

            // grab the first question from the in‐memory session
            var firstQuestion = _interviewService
                .GetQuestions(sessionId)
                .FirstOrDefault()?.Text
                ?? "No question generated.";

            return Ok(new
            {
                message = "Interview initialized",
                sessionId,
                firstQuestion
            });
        }

        /// <summary>
        /// Gets the next question in the current interview session.
        /// </summary>
        [HttpGet("question")]
        public async Task<IActionResult> GetNextQuestion([FromQuery, Required] string sessionId)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { error = "SessionId is required." });

            var nextQuestion = await _interviewService.GetNextQuestionAsync(sessionId);

            if (string.IsNullOrWhiteSpace(nextQuestion))
                return Ok(new { message = "Interview complete" });

            return Ok(new
            {
                index = _interviewService.CurrentIndex(sessionId),
                question = nextQuestion
            });
        }

        /// <summary>
        /// Submits an answer to the current question.
        /// </summary>
        [HttpPost("answer")]
        public async Task<IActionResult> SubmitAnswer([FromBody] AnswerDto answerDto)
        {
            // AnswerDto now includes SessionId, Question, AudioBase64, Transcript
            if (answerDto == null
                || string.IsNullOrWhiteSpace(answerDto.SessionId)
                || string.IsNullOrWhiteSpace(answerDto.Question))
            {
                return BadRequest(new { error = "Invalid answer payload." });
            }

            bool ok = await _interviewService.SubmitAnswerAsync(answerDto.SessionId, answerDto);
            if (!ok)
                return BadRequest(new { error = "No more questions." });

            return Ok(new
            {
                message = "Answer recorded",
                index = _interviewService.CurrentIndex(answerDto.SessionId)
            });
        }

        /// <summary>
        /// Completes the current interview and returns a brief summary.
        /// </summary>
        [HttpPost("complete")]
        public async Task<IActionResult> CompleteInterview([FromQuery, Required] string sessionId)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { error = "SessionId is required." });

            var summary = await _interviewService.GetCompletionSummaryAsync(sessionId);
            return Ok(summary);
        }

        /// <summary>
        /// Generates and returns a detailed interview report after evaluation.
        /// </summary>
        [HttpGet("report")]
        public async Task<IActionResult> GetReport([FromQuery, Required] string sessionId)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { error = "SessionId is required." });

            var report = await _interviewService.GenerateReportAsync(sessionId);
            return Ok(report);
        }

        /// <summary>
        /// Fetches all interview reports associated with a given email address.
        /// </summary>
        [HttpGet("reports")]
        public async Task<IActionResult> GetReportsByEmail([FromQuery, Required] string email)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { error = "Email is required." });

            var reports = await _interviewService.GetReportsByEmailAsync(email);
            return Ok(reports);
        }

        /// <summary>
        /// Fetches a specific interview report by its unique ID.
        /// </summary>
        [HttpGet("report/{id}")]
        public async Task<IActionResult> GetReportById([FromRoute] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { error = "Report ID is required." });

            var report = await _interviewService.GetReportByIdAsync(id);
            if (report == null)
                return NotFound(new { error = "Report not found." });

            return Ok(report);
        }

        /// <summary>
        /// Health check endpoint to verify if the API is running.
        /// </summary>
        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
            => Ok("Healthy");
    }
}
