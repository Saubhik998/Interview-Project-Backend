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
    /// <summary>
    /// Service for managing interview sessions, handling questions, answers, audio storage, and evaluation.
    /// </summary>
    public class InterviewService : IInterviewService
    {
        private const int MaxQuestions = 5;
        private const int MaxAudioSizeBytes = 5 * 1024 * 1024; // 5 MB

        private readonly MongoDbContext _dbContext;
        private readonly IApiClient _fastApiClient;
        private readonly HttpClient _httpClient;
        private InterviewSession _session = new();

        /// <summary>
        /// Constructor for InterviewService.
        /// </summary>
        /// <param name="dbContext">MongoDB database context.</param>
        /// <param name="fastApiClient">Client to fetch AI-generated questions.</param>
        /// <param name="httpClientFactory">HTTP client factory for making API requests.</param>
        public InterviewService(MongoDbContext dbContext, IApiClient fastApiClient, IHttpClientFactory httpClientFactory)
        {
            _dbContext = dbContext;
            _fastApiClient = fastApiClient;
            _httpClient = httpClientFactory.CreateClient();
        }

        /// <summary>
        /// Initializes a new interview session with job description and candidate email.
        /// </summary>
        public async Task InitializeSessionAsync(string jobDescription, string email)
        {
            string firstQuestion = await _fastApiClient.GetFirstQuestionAsync(jobDescription);

            _session = new InterviewSession
            {
                Email = email.Trim().ToLowerInvariant(),
                JobDescription = jobDescription.Trim(),
                CurrentIndex = 0,
                Answers = new List<Answer>(),
                Questions = new List<Question>
                {
                    new Question { Text = firstQuestion }
                }
            };

            await _dbContext.Sessions.InsertOneAsync(_session);
        }

        /// <summary>
        /// Retrieves the next question based on the previous answer.
        /// </summary>
        /// <returns>The next question text, or null if limit reached.</returns>
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
                    await _dbContext.Sessions.ReplaceOneAsync(filter, _session);
                    return nextQuestion;
                }

                return null;
            }

            return _session.Questions[_session.CurrentIndex].Text;
        }

        /// <summary>
        /// Submits the candidate's answer, stores audio in GridFS, and updates the session.
        /// </summary>
        /// <param name="answerDto">Answer DTO containing audio and transcript.</param>
        /// <returns>True if submission is successful.</returns>
        public async Task<bool> SubmitAnswerAsync(AnswerDto answerDto)
        {
            if (_session.CurrentIndex >= _session.Questions.Count)
                return false;

            byte[] audioBytes;

            try
            {
                audioBytes = Convert.FromBase64String(answerDto.AudioBase64);
            }
            catch
            {
                throw new InvalidOperationException("Invalid audio base64 string.");
            }

            if (audioBytes.Length > MaxAudioSizeBytes)
                throw new InvalidOperationException("Audio file too large (limit: 5MB).");

            string filename = $"answer_{DateTime.UtcNow.Ticks}.webm";

            ObjectId fileId;
            using (var stream = new MemoryStream(audioBytes))
            {
                fileId = await _dbContext.GridFsBucket.UploadFromStreamAsync(filename, stream);
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
            await _dbContext.Sessions.ReplaceOneAsync(filterUpdate, _session);

            return true;
        }

        /// <summary>
        /// Gets the list of questions for the current session.
        /// </summary>
        public List<Question> GetQuestions() => _session.Questions;

        /// <summary>
        /// Gets the current index in the session.
        /// </summary>
        public int CurrentIndex => _session.CurrentIndex;

        /// <summary>
        /// Returns a basic summary after interview completion.
        /// </summary>
        public async Task<object> GetCompletionSummaryAsync()
        {
            return new
            {
                message = "Interview completed",
                totalQuestions = _session.Questions.Count,
                totalAnswers = _session.Answers.Count
            };
        }

        /// <summary>
        /// Evaluates answers using the AI API and generates an interview report.
        /// </summary>
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

            await _dbContext.Reports.InsertOneAsync(dbReport);

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

        /// <summary>
        /// Retrieves all reports by candidate email.
        /// </summary>
        public async Task<List<InterviewReport>> GetReportsByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                return new List<InterviewReport>();

            var sanitizedEmail = email.Trim().ToLowerInvariant();
            var filter = Builders<InterviewReport>.Filter.Eq(r => r.Email, sanitizedEmail);
            return await _dbContext.Reports.Find(filter).ToListAsync();
        }

        /// <summary>
        /// Retrieves a specific report by its MongoDB ObjectId.
        /// </summary>
        public async Task<InterviewReport?> GetReportByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
                return null;

            var filter = Builders<InterviewReport>.Filter.Eq("_id", objectId);
            return await _dbContext.Reports.Find(filter).FirstOrDefaultAsync();
        }
    }
}
