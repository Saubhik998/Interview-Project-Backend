using Microsoft.AspNetCore.Mvc;
using AudioInterviewer.API.Models;

namespace AudioInterviewer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InterviewController : ControllerBase
    {
        private static List<string> _questions = new()
        {
            "Tell me about yourself.",
            "What are your strengths?",
            "Why do you want this job?"
        };

        private static int _currentIndex = 0;

        private static List<object> _answers = new();

        private static string _jobDescription = "";

        /// <summary>
        /// Initializes an interview session with the provided job description.
        /// Returns a list of mock questions.
        /// </summary>
        [HttpPost("init")]
        public IActionResult InitializeInterview([FromBody] string jobDescription)
        {
            _jobDescription = jobDescription;
            _currentIndex = 0;
            _answers.Clear(); // Reset previous answers if any

            return Ok(new
            {
                message = "Interview initialized",
                jd = jobDescription,
                questions = _questions
            });
        }

        /// <summary>
        /// Gets the next interview question in the sequence.
        /// </summary>
        [HttpGet("question")]
        public IActionResult GetNextQuestion()
        {
            if (_currentIndex >= _questions.Count)
                return Ok(new { message = "Interview complete" });

            var question = _questions[_currentIndex];
            return Ok(new { index = _currentIndex, question });
        }

        /// <summary>
        /// Stores an answer for the current question.
        /// </summary>
        [HttpPost("answer")]
        public IActionResult SubmitAnswer([FromBody] AnswerDto answer)
        {
            if (_currentIndex >= _questions.Count)
                return BadRequest("No more questions.");

            _answers.Add(new
            {
                question = _questions[_currentIndex],
                answer.audioUrl,
                answer.transcript
            });

            _currentIndex++;

            return Ok(new { message = "Answer recorded", index = _currentIndex });
        }

        /// <summary>
        /// Completes the interview and returns a basic summary.
        /// </summary>
        [HttpPost("complete")]
        public IActionResult CompleteInterview()
        {
            return Ok(new
            {
                message = "Interview completed",
                totalQuestions = _questions.Count,
                totalAnswers = _answers.Count
            });
        }

        /// <summary>
        /// Returns a mock report for the completed interview.
        /// </summary>
        [HttpGet("report")]
        public IActionResult GetReport()
        {
            var report = new
            {
                candidateFit = 78,
                strengths = new[] { "Good communication", "Clear responses" },
                improvementAreas = new[] { "More technical detail", "Confidence" },
                suggestedFollowUp = "Schedule technical round",
                answers = _answers
            };

            return Ok(report);
        }
    }

    /// <summary>
    /// DTO for receiving answer data.
    /// </summary>
    public class AnswerDto
    {
        public string audioUrl { get; set; } = "";
        public string transcript { get; set; } = "";
    }
}
