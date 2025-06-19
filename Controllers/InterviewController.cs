using Microsoft.AspNetCore.Mvc;
using AudioInterviewer.API.Models;
using AudioInterviewer.API.Services;

namespace AudioInterviewer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InterviewController : ControllerBase
    {
        private readonly InterviewService _interviewService;

        public InterviewController(InterviewService interviewService)
        {
            _interviewService = interviewService;
        }

        /// <summary>
        /// Initializes the interview session with a given Job Description.
        /// </summary>
        [HttpPost("init")]
        public IActionResult InitializeInterview([FromBody] string jobDescription)
        {
            _interviewService.InitializeSession(jobDescription);
            return Ok(new
            {
                message = "Interview initialized",
                jd = jobDescription,
                questions = _interviewService.GetQuestions().Select(q => q.Text).ToList()
            });
        }

        /// <summary>
        /// Retrieves the next question in the session.
        /// </summary>
        [HttpGet("question")]
        public IActionResult GetNextQuestion()
        {
            var question = _interviewService.GetNextQuestion();
            if (question == null)
                return Ok(new { message = "Interview complete" });

            return Ok(new { index = _interviewService.CurrentIndex, question = question.Text });
        }

        /// <summary>
        /// Stores the candidate’s answer for the current question.
        /// </summary>
        [HttpPost("answer")]
        public IActionResult SubmitAnswer([FromBody] AnswerDto answerDto)
        {
            bool success = _interviewService.SubmitAnswer(answerDto);
            if (!success)
                return BadRequest("No more questions.");

            return Ok(new { message = "Answer recorded", index = _interviewService.CurrentIndex });
        }

        /// <summary>
        /// Completes the interview.
        /// </summary>
        [HttpPost("complete")]
        public IActionResult CompleteInterview()
        {
            var summary = _interviewService.GetCompletionSummary();
            return Ok(summary);
        }

        /// <summary>
        /// Returns a mock interview report.
        /// </summary>
        [HttpGet("report")]
        public IActionResult GetReport()
        {
            return Ok(_interviewService.GenerateReport());
        }
    }
}
