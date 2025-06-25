using AudioInterviewer.API.Data;
using AudioInterviewer.API.Models;
using MongoDB.Driver;

namespace AudioInterviewer.API.Services
{
    /// <summary>
    /// Service that manages interview logic and persists data using MongoDB.
    /// </summary>
    public class InterviewService
    {
        private readonly MongoDbContext _dbContext;
        private InterviewSession _session = new();

        public InterviewService(MongoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Initializes a new interview session and stores it in MongoDB.
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

            _dbContext.Sessions.InsertOne(_session);
        }

        /// <summary>
        /// Returns the next question or null if interview is complete.
        /// </summary>
        public Question? GetNextQuestion()
        {
            if (_session.CurrentIndex >= _session.Questions.Count)
                return null;

            return _session.Questions[_session.CurrentIndex];
        }

        /// <summary>
        /// Saves answer and updates the session document in MongoDB.
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

            // Update session in MongoDB
            var filter = Builders<InterviewSession>.Filter.Eq(s => s.Id, _session.Id);
            _dbContext.Sessions.ReplaceOne(filter, _session);

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

        /// <summary>
        /// Generates the final report in the format expected by the frontend.
        /// </summary>
        public object GenerateReport()
        {
            return new
            {
                jd = _session.JobDescription ?? "No JD provided",
                score = 78,
                questions = _session.Questions.Select(q => q.Text).ToList(),
                answers = _session.Answers.Select(a => new
                {
                    transcript = a.Transcript ?? "",
                    audio = a.AudioUrl ?? ""
                }).ToList(),
                strengths = new List<string> { "Good communication", "Clear responses" },
                improvements = new List<string> { "More technical detail", "Confidence" },
                followUps = new List<string> { "Schedule technical round" }
            };
        }
    }
}
