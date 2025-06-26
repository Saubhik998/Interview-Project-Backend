using AudioInterviewer.API.Data;
using AudioInterviewer.API.Models;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System.Text;
using MongoDB.Bson;

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

        public Question? GetNextQuestion()
        {
            if (_session.CurrentIndex >= _session.Questions.Count)
                return null;

            return _session.Questions[_session.CurrentIndex];
        }

        public bool SubmitAnswer(AnswerDto answerDto)
        {
            if (_session.CurrentIndex >= _session.Questions.Count)
                return false;

            // Convert base64 to bytes
            byte[] audioBytes = Convert.FromBase64String(answerDto.AudioBase64);

            // Generate a filename and upload to GridFS
            string filename = $"answer_{DateTime.UtcNow.Ticks}.webm";
            ObjectId fileId;
            using (var stream = new MemoryStream(audioBytes))
            {
                fileId = _dbContext.GridFsBucket.UploadFromStream(filename, stream);
            }

            // The "audio URL" now becomes an endpoint to stream the audio by fileId
            string audioUrl = $"/api/audio/{fileId}";

            var answer = new Answer
            {
                Question = answerDto.Question,
                Transcript = answerDto.Transcript,
                AudioUrl = audioUrl
            };

            _session.Answers.Add(answer);
            _session.CurrentIndex++;

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

        public object GenerateReport()
        {
            return new
            {
                jd = _session.JobDescription ?? "No JD provided",
                score = 78,
                questions = _session.Questions.Select(q => q.Text).ToList(),
                answers = _session.Answers.Select(a => new
                {
                    question = a.Question,
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
