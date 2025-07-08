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

        // POST: /api/interview/init
        [HttpPost("init")]
        public async Task<IActionResult> InitializeInterview([FromBody] string jobDescription)
        {
            await _interviewService.InitializeSessionAsync(jobDescription);

            return Ok(new
            {
                message = "Interview initialized",
                jd = jobDescription,
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

        // POST: /api/interview/complete
        [HttpPost("complete")]
        public IActionResult CompleteInterview()
        {
            var summary = _interviewService.GetCompletionSummary();
            return Ok(summary);
        }

        // GET: /api/interview/report
        [HttpGet("report")]
        public async Task<IActionResult> GetReport()
        {
            var report = await _interviewService.GenerateReportAsync();
            return Ok(report);
        }
    }
}
