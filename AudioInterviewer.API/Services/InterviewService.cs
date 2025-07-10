using AudioInterviewer.API.Data;
using AudioInterviewer.API.Models;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MongoDB.Bson;
using System.Text.Json;
using System.Net.Http.Json;
using AudioInterviewer.API.Services.External;

namespace AudioInterviewer.API.Services
{
    public class InterviewService
    {
        private const int MaxQuestions = 5;

        private readonly MongoDbContext _dbContext;
        private readonly FastApiClient _fastApiClient;
        private readonly HttpClient _httpClient;
        private InterviewSession _session = new();

        public InterviewService(MongoDbContext dbContext, FastApiClient fastApiClient, IHttpClientFactory httpClientFactory)
        {
            _dbContext = dbContext;
            _fastApiClient = fastApiClient;
            _httpClient = httpClientFactory.CreateClient();
        }

        // Initialize interview session with JD and Email
        public async Task InitializeSessionAsync(string jobDescription, string email)
        {
            string firstQuestion = await _fastApiClient.GetFirstQuestionAsync(jobDescription);

            _session = new InterviewSession
            {
                Email = email,
                JobDescription = jobDescription,
                CurrentIndex = 0,
                Answers = new List<Answer>(),
                Questions = new List<Question>
                {
                    new Question { Text = firstQuestion }
                }
            };

            _dbContext.Sessions.InsertOne(_session);
        }

        public async Task<string?> GetNextQuestionAsync()
        {
            if (_session.CurrentIndex == 0)
                return _session.Questions[0].Text;

            if (_session.Questions.Count >= MaxQuestions)
                return null;

            if (_session.CurrentIndex >= _session.Questions.Count)
            {
                var previousAnswer = _session.Answers.Last();
                string nextQuestion = await _fastApiClient.GetNextQuestionAsync(
                    _session.JobDescription,
                    previousAnswer.Question,
                    previousAnswer.Transcript ?? ""
                );

                if (!string.IsNullOrWhiteSpace(nextQuestion))
                {
                    _session.Questions.Add(new Question { Text = nextQuestion });

                    var filter = Builders<InterviewSession>.Filter.Eq(s => s.Id, _session.Id);
                    _dbContext.Sessions.ReplaceOne(filter, _session);
                    return nextQuestion;
                }

                return null;
            }

            return _session.Questions[_session.CurrentIndex].Text;
        }

        public bool SubmitAnswer(AnswerDto answerDto)
        {
            if (_session.CurrentIndex >= _session.Questions.Count)
                return false;

            byte[] audioBytes = Convert.FromBase64String(answerDto.AudioBase64);
            string filename = $"answer_{DateTime.UtcNow.Ticks}.webm";

            ObjectId fileId;
            using (var stream = new MemoryStream(audioBytes))
            {
                fileId = _dbContext.GridFsBucket.UploadFromStream(filename, stream);
            }

            string audioUrl = $"/api/audio/{fileId}";

            var answer = new Answer
            {
                Question = answerDto.Question,
                Transcript = answerDto.Transcript,
                AudioUrl = audioUrl
            };

            _session.Answers.Add(answer);
            _session.CurrentIndex++;

            var filterUpdate = Builders<InterviewSession>.Filter.Eq(s => s.Id, _session.Id);
            _dbContext.Sessions.ReplaceOne(filterUpdate, _session);

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

        public async Task<object> GenerateReportAsync()
        {
            var aiPayload = new
            {
                jd = _session.JobDescription ?? "",
                questions = _session.Questions.Select(q => q.Text).ToList(),
                answers = _session.Answers.Select(a => a.Transcript ?? "").ToList()
            };

            var aiResponse = await _httpClient.PostAsJsonAsync("http://localhost:8000/api/evaluate", aiPayload);
            if (!aiResponse.IsSuccessStatusCode)
                throw new Exception("Failed to get evaluation from AI service");

            var aiReport = await aiResponse.Content.ReadFromJsonAsync<JsonElement>();

            var enrichedAnswers = _session.Questions.Select(q =>
            {
                var matchingAnswer = _session.Answers.FirstOrDefault(a => a.Question == q.Text);
                return new
                {
                    question = q.Text,
                    transcript = matchingAnswer?.Transcript ?? "",
                    audio = matchingAnswer?.AudioUrl ?? ""
                };
            }).ToList();

            var dbReport = new InterviewReport
            {
                Email = _session.Email ?? "",
                JobDescription = _session.JobDescription ?? "",
                CandidateFitScore = aiReport.GetProperty("score").GetInt32(),
                Strengths = aiReport.GetProperty("strengths").EnumerateArray().Select(s => s.GetString() ?? "").ToList(),
                ImprovementAreas = aiReport.GetProperty("improvements").EnumerateArray().Select(s => s.GetString() ?? "").ToList(),
                SuggestedFollowUp = aiReport.GetProperty("followUps").EnumerateArray().Select(f => f.GetString() ?? "").ToList(),
                Answers = _session.Answers
            };

            _dbContext.Reports.InsertOne(dbReport);

            return new
            {
                jd = aiReport.GetProperty("jd").GetString() ?? "",
                score = dbReport.CandidateFitScore,
                questions = aiReport.GetProperty("questions").EnumerateArray().Select(q => q.GetString() ?? "").ToList(),
                answers = enrichedAnswers,
                strengths = dbReport.Strengths,
                improvements = dbReport.ImprovementAreas,
                followUps = dbReport.SuggestedFollowUp
            };
        }

        // Fetch all reports by candidate email
        public async Task<List<InterviewReport>> GetReportsByEmailAsync(string email)
        {
            var filter = Builders<InterviewReport>.Filter.Eq(r => r.Email, email);
            return await _dbContext.Reports.Find(filter).ToListAsync();
        }

        // Fetch one report by MongoDB ObjectId
        public async Task<InterviewReport?> GetReportByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
                return null;

            var filter = Builders<InterviewReport>.Filter.Eq("_id", objectId);
            return await _dbContext.Reports.Find(filter).FirstOrDefaultAsync();
        }
    }
}
