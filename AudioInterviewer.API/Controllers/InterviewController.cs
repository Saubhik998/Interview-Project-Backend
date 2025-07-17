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
            /// <summary>
            /// Candidate's email address.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; } = "";

            /// <summary>
            /// Job description provided by the recruiter or system.
            /// </summary>
            [Required]
            [MinLength(10, ErrorMessage = "Job description must be at least 10 characters long.")]
            public string JobDescription { get; set; } = "";
        }

        /// <summary>
        /// Initializes a new interview session with the given email and job description.
        /// </summary>
        /// <param name="request">Initialization request containing email and job description.</param>
        [HttpPost("init")]
        public async Task<IActionResult> InitializeInterview([FromBody] InitRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { error = "Invalid input", details = ModelState });

            await _interviewService.InitializeSessionAsync(request.JobDescription, request.Email);

            return Ok(new
            {
                message = "Interview initialized",
                jd = request.JobDescription,
                firstQuestion = _interviewService.GetQuestions().FirstOrDefault()?.Text ?? "No question generated."
            });
        }

        /// <summary>
        /// Gets the next question in the current interview session.
        /// </summary>
        [HttpGet("question")]
        public async Task<IActionResult> GetNextQuestion()
        {
            var nextQuestion = await _interviewService.GetNextQuestionAsync();

            if (string.IsNullOrWhiteSpace(nextQuestion))
                return Ok(new { message = "Interview complete" });

            return Ok(new { index = _interviewService.CurrentIndex, question = nextQuestion });
        }

        /// <summary>
        /// Submits an answer to the current question.
        /// </summary>
        /// <param name="answerDto">Answer payload including transcript and base64 audio.</param>
        [HttpPost("answer")]
        public async Task<IActionResult> SubmitAnswer([FromBody] AnswerDto answerDto)
        {
            if (answerDto == null || string.IsNullOrWhiteSpace(answerDto.Question))
                return BadRequest(new { error = "Invalid answer payload." });

            bool success = await _interviewService.SubmitAnswerAsync(answerDto);

            if (!success)
                return BadRequest(new { error = "No more questions." });

            return Ok(new { message = "Answer recorded", index = _interviewService.CurrentIndex });
        }

        /// <summary>
        /// Completes the current interview and returns a summary.
        /// </summary>
        [HttpPost("complete")]
        public async Task<IActionResult> CompleteInterview()
        {
            var summary = await _interviewService.GetCompletionSummaryAsync();
            return Ok(summary);
        }

        /// <summary>
        /// Generates and returns a detailed interview report after evaluation.
        /// </summary>
        [HttpGet("report")]
        public async Task<IActionResult> GetReport()
        {
            var report = await _interviewService.GenerateReportAsync();
            return Ok(report);
        }

        /// <summary>
        /// Fetches all interview reports associated with a given email address.
        /// </summary>
        /// <param name="email">Candidate email address.</param>
        [HttpGet("reports")]
        public async Task<IActionResult> GetReportsByEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { error = "Email is required." });

            var reports = await _interviewService.GetReportsByEmailAsync(email);
            return Ok(reports);
        }

        /// <summary>
        /// Fetches a specific interview report by its unique ID.
        /// </summary>
        /// <param name="id">MongoDB ObjectId string of the report.</param>
        [HttpGet("report/{id}")]
        public async Task<IActionResult> GetReportById(string id)
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
        /// <returns>200 OK with a health status message.</returns>
        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
        {
            return Ok("Healthy");
        }
    }
}
