using Microsoft.AspNetCore.Mvc;
using AudioInterviewer.API.Models;
using AudioInterviewer.API.Services;
using System.Threading.Tasks;

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

        public class InitRequest
        {
            public string Email { get; set; } = "";
            public string JobDescription { get; set; } = "";
        }

        //  POST: /api/interview/init
        [HttpPost("init")]
        public async Task<IActionResult> InitializeInterview([FromBody] InitRequest request)
        {
            await _interviewService.InitializeSessionAsync(
                request.JobDescription,
                request.Email
            );

            return Ok(new
            {
                message = "Interview initialized",
                jd = request.JobDescription,
                firstQuestion = _interviewService.GetQuestions().FirstOrDefault()?.Text ?? "No question generated."
            });
        }

        // GET: /api/interview/question
        [HttpGet("question")]
        public async Task<IActionResult> GetNextQuestion()
        {
            var nextQuestion = await _interviewService.GetNextQuestionAsync();

            if (string.IsNullOrWhiteSpace(nextQuestion))
                return Ok(new { message = "Interview complete" });

            return Ok(new { index = _interviewService.CurrentIndex, question = nextQuestion });
        }

        // POST: /api/interview/answer
        [HttpPost("answer")]
        public IActionResult SubmitAnswer([FromBody] AnswerDto answerDto)
        {
            bool success = _interviewService.SubmitAnswer(answerDto);
            if (!success)
                return BadRequest("No more questions.");

            return Ok(new { message = "Answer recorded", index = _interviewService.CurrentIndex });
        }

        //  POST: /api/interview/complete
        [HttpPost("complete")]
        public IActionResult CompleteInterview()
        {
            var summary = _interviewService.GetCompletionSummary();
            return Ok(summary);
        }

        //  GET: /api/interview/report
        [HttpGet("report")]
        public async Task<IActionResult> GetReport()
        {
            var report = await _interviewService.GenerateReportAsync();
            return Ok(report);
        }

        //  NEW: GET /api/interview/reports?email=user@example.com
        [HttpGet("reports")]
        public async Task<IActionResult> GetReportsByEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email is required.");

            var reports = await _interviewService.GetReportsByEmailAsync(email);
            return Ok(reports);
        }

                //  NEW: GET /api/interview/report/{id}
        [HttpGet("report/{id}")]
        public async Task<IActionResult> GetReportById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Report ID is required.");

            var report = await _interviewService.GetReportByIdAsync(id);

            if (report == null)
                return NotFound("Report not found.");

            return Ok(report);
        }
    


    }
}
