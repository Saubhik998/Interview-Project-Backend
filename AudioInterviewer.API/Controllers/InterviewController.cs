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
        /// <param name="interviewService">Service responsible for interview business logic.</param>
        public InterviewController(IInterviewService interviewService)
        {
            _interviewService = interviewService;
        }

        /// <summary>
        /// Request payload for initializing an interview.
        /// </summary>
        public class InitRequest
        {
            /// <summary>
            /// Email of the interviewee. Required and must be a valid email address.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            /// <summary>
            /// Job description related to the interview. Minimum length 10 characters.
            /// </summary>
            [Required]
            [MinLength(10, ErrorMessage = "Job description must be at least 10 characters long.")]
            public string JobDescription { get; set; } = string.Empty;
        }

        /// <summary>
        /// Initializes a new interview session with the given email and job description.
        /// </summary>
        /// <param name="request">The initialization request payload containing email and job description.</param>
        /// <returns>
        /// Returns OK with a new session ID and the first question if successful;
        /// otherwise, returns BadRequest with validation errors.
        /// </returns>
        /// <response code="200">Interview session initialized successfully.</response>
        /// <response code="400">Request payload validation failed.</response>
        [HttpPost("init")]
        public async Task<IActionResult> InitializeInterview([FromBody] InitRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { error = "Invalid input", details = ModelState });

            var sessionId = await _interviewService.InitializeSessionAsync(request.JobDescription, request.Email);

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
        /// <param name="sessionId">The unique identifier for the interview session.</param>
        /// <returns>
        /// Returns the next question with its index if available;
        /// else indicates the interview is complete.
        /// Returns BadRequest if sessionId is missing or invalid.
        /// </returns>
        /// <response code="200">Next question retrieved or interview marked complete.</response>
        /// <response code="400">SessionId missing or invalid.</response>
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
        /// Submits an answer to the current question in the interview session.
        /// </summary>
        /// <param name="answerDto">Answer data including session ID, question, audio, and transcript.</param>
        /// <returns>
        /// Returns OK on successful recording of answer;
        /// returns BadRequest if input is invalid or if no more questions remain.
        /// </returns>
        /// <response code="200">Answer recorded successfully.</response>
        /// <response code="400">Invalid answer payload or no more questions.</response>
        [HttpPost("answer")]
        public async Task<IActionResult> SubmitAnswer([FromBody] AnswerDto answerDto)
        {
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
        /// Completes the current interview session and returns a brief summary.
        /// </summary>
        /// <param name="sessionId">The interview session identifier.</param>
        /// <returns>
        /// Returns interview summary on successful completion;
        /// returns BadRequest if sessionId is missing or invalid.
        /// </returns>
        /// <response code="200">Interview completed with summary returned.</response>
        /// <response code="400">SessionId missing or invalid.</response>
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
        /// <param name="sessionId">The interview session identifier.</param>
        /// <returns>
        /// Returns the detailed report if available;
        /// returns BadRequest if sessionId is missing or invalid.
        /// </returns>
        /// <response code="200">Detailed report returned successfully.</response>
        /// <response code="400">SessionId missing or invalid.</response>
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
        /// <param name="email">Email address of the interviewee.</param>
        /// <returns>
        /// Returns a collection of reports related to the specified email;
        /// returns BadRequest if email is missing or invalid.
        /// </returns>
        /// <response code="200">Reports retrieved successfully.</response>
        /// <response code="400">Email missing or invalid.</response>
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
        /// <param name="id">Unique identifier of the report.</param>
        /// <returns>
        /// Returns the report if found;
        /// returns BadRequest if the ID is missing or NotFound if no report matches the ID.
        /// </returns>
        /// <response code="200">Report retrieved successfully.</response>
        /// <response code="400">Report ID missing or invalid.</response>
        /// <response code="404">Report not found with the given ID.</response>
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
        /// <returns>Returns "Healthy" if the API is operational.</returns>
        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
            => Ok("Healthy");
    }
}
