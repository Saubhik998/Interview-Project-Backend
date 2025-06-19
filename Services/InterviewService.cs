using AudioInterviewer.API.Models;

namespace AudioInterviewer.API.Services
{
    /// <summary>
    /// Service that manages in-memory interview logic and state.
    /// </summary>
    public class InterviewService
    {
        private InterviewSession _session = new();

        /// <summary>
        /// Initializes a new interview session with a given job description.
        /// </summary>
        public void InitializeSession(string jobDescription)
        {
            _session = new InterviewSession
            {
                JobDescription = jobDescription,
                CurrentIndex = 0,
                Answers = new List<Answer>(),
                Questions = new List<Question>
                {
                    new Question { Text = "Tell me about yourself." },
                    new Question { Text = "What are your strengths?" },
                    new Question { Text = "Why do you want this job?" }
                }
            };
        }

        /// <summary>
        /// Returns the next question or null if done.
        /// </summary>
        public Question? GetNextQuestion()
        {
            if (_session.CurrentIndex >= _session.Questions.Count)
                return null;

            return _session.Questions[_session.CurrentIndex];
        }

        /// <summary>
        /// Submits an answer and increments index.
        /// </summary>
        public bool SubmitAnswer(AnswerDto answerDto)
        {
            if (_session.CurrentIndex >= _session.Questions.Count)
                return false;

            var answer = new Answer
            {
                Question = _session.Questions[_session.CurrentIndex].Text,
                AudioUrl = answerDto.AudioUrl,
                Transcript = answerDto.Transcript
            };

            _session.Answers.Add(answer);
            _session.CurrentIndex++;

            return true;
        }

        public List<Question> GetQuestions() => _session.Questions;

        public int CurrentIndex => _session.CurrentIndex;

        public object GetCompletionSummary() => new
        {
            message = "Interview completed",
            totalQuestions = _session.Questions.Count,
            totalAnswers = _session.Answers.Count
        };

        public InterviewReport GenerateReport() => new()
        {
            CandidateFitScore = 78,
            Strengths = new List<string> { "Good communication", "Clear responses" },
            ImprovementAreas = new List<string> { "More technical detail", "Confidence" },
            SuggestedFollowUp = "Schedule technical round",
            Answers = _session.Answers
        };
    }
}
