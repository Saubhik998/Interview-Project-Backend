using AudioInterviewer.API.Data;
using AudioInterviewer.API.Models;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MongoDB.Bson;
using System.Text.Json;
using System.Net.Http.Json;
using AudioInterviewer.API.Services.External;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AudioInterviewer.API.Services
{
    /// <summary>
    /// Service for managing interview sessions, handling questions, answers, audio storage, and evaluation.
    /// Implements the <see cref="IInterviewService"/> interface.
    /// </summary>
    public class InterviewService : IInterviewService
    {
        /// <summary>
        /// The maximum number of questions allowed per interview session.
        /// </summary>
        private const int MaxQuestions = 5;

        /// <summary>
        /// The maximum allowed audio size in bytes (5MB).
        /// </summary>
        private const int MaxAudioSizeBytes = 5 * 1024 * 1024; // 5 MB

        private readonly IMongoDbContext _dbContext;
        private readonly IApiClient _fastApiClient;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewService"/> class.
        /// </summary>
        /// <param name="dbContext">MongoDB context for sessions and reports.</param>
        /// <param name="fastApiClient">External API client for LLM question generation.</param>
        /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
        public InterviewService(IMongoDbContext dbContext, IApiClient fastApiClient, IHttpClientFactory httpClientFactory)
        {
            _dbContext      = dbContext;
            _fastApiClient  = fastApiClient;
            _httpClient     = httpClientFactory.CreateClient();
        }

        /// <summary>
        /// Creates a new interview session in the database and returns its session ID.
        /// </summary>
        /// <param name="jobDescription">The job description for the interview.</param>
        /// <param name="email">Candidate's email address.</param>
        /// <returns>The newly created session ID as a string.</returns>
        public async Task<string> InitializeSessionAsync(string jobDescription, string email)
        {
            var firstQuestion = await _fastApiClient.GetFirstQuestionAsync(jobDescription);
            var session = new InterviewSession
            {
                Email           = email.Trim().ToLowerInvariant(),
                JobDescription  = jobDescription.Trim(),
                CurrentIndex    = 0,
                Answers         = new List<Answer>(),
                Questions       = new List<Question> { new Question { Text = firstQuestion } }
            };

            await _dbContext.Sessions.InsertOneAsync(session);
            return session.Id;
        }

        /// <summary>
        /// Retrieves the next question for the given session, or null if the interview is complete.
        /// </summary>
        /// <param name="sessionId">The interview session ID.</param>
        /// <returns>
        /// The next question text as a string, or null if the session is completed or not found.
        /// </returns>
        public async Task<string?> GetNextQuestionAsync(string sessionId)
        {
            var session = await LoadSessionAsync(sessionId);
            if (session == null) return null;

            if (session.CurrentIndex == 0)
                return session.Questions[0].Text;

            if (session.Questions.Count >= MaxQuestions)
                return null;

            if (session.CurrentIndex >= session.Questions.Count)
            {
                var prevAns = session.Answers.Last();
                var nextQ   = await _fastApiClient.GetNextQuestionAsync(
                    session.JobDescription, prevAns.Question, prevAns.Transcript ?? string.Empty);
                if (!string.IsNullOrWhiteSpace(nextQ))
                {
                    session.Questions.Add(new Question { Text = nextQ });
                    await SaveSessionAsync(session);
                    return nextQ;
                }
                return null;
            }

            return session.Questions[session.CurrentIndex].Text;
        }

        /// <summary>
        /// Submits an answer, stores the audio in GridFS, and updates the session index.
        /// </summary>
        /// <param name="sessionId">The interview session ID.</param>
        /// <param name="answerDto">Answer payload including Base64 audio and transcript.</param>
        /// <returns>
        /// True if the answer was accepted and processed; otherwise, false.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the audio is too large or the Base64 string is invalid.
        /// </exception>
        public async Task<bool> SubmitAnswerAsync(string sessionId, AnswerDto answerDto)
        {
            var session = await LoadSessionAsync(sessionId);
            if (session == null || session.CurrentIndex >= session.Questions.Count) return false;

            byte[] audio;
            try
            {
                audio = Convert.FromBase64String(answerDto.AudioBase64);
            }
            catch
            {
                throw new InvalidOperationException("Invalid audio base64 string.");
            }

            if (audio.Length > MaxAudioSizeBytes)
                throw new InvalidOperationException("Audio file too large (limit: 5MB).");

            var filename = $"answer_{DateTime.UtcNow.Ticks}.webm";
            ObjectId fileId;
            using (var ms = new MemoryStream(audio))
            {
                fileId = await _dbContext.GridFsBucket.UploadFromStreamAsync(filename, ms);
            }

            var answer = new Answer
            {
                Question    = answerDto.Question,
                Transcript  = answerDto.Transcript,
                AudioUrl    = $"/api/audio/{fileId}"
            };
            session.Answers.Add(answer);
            session.CurrentIndex++;
            await SaveSessionAsync(session);
            return true;
        }

        /// <summary>
        /// Retrieves the list of questions posted so far in a session.
        /// </summary>
        /// <param name="sessionId">The interview session ID.</param>
        /// <returns>
        /// List of <see cref="Question"/> objects asked so far; empty list if session not found.
        /// </returns>
        public List<Question> GetQuestions(string sessionId)
        {
            var session = _dbContext.Sessions.Find(s => s.Id == sessionId).FirstOrDefault();
            return session?.Questions ?? new List<Question>();
        }

        /// <summary>
        /// Gets the current question index for a session.
        /// </summary>
        /// <param name="sessionId">The interview session ID.</param>
        /// <returns>Zero-based current index. Returns 0 if session not found.</returns>
        public int CurrentIndex(string sessionId)
        {
            var session = _dbContext.Sessions.Find(s => s.Id == sessionId).FirstOrDefault();
            return session?.CurrentIndex ?? 0;
        }

        /// <summary>
        /// Gets a summary of completion status for a session.
        /// </summary>
        /// <param name="sessionId">The interview session ID.</param>
        /// <returns>
        /// Object containing a summary message, and counts of questions and answers.
        /// </returns>
        public async Task<object> GetCompletionSummaryAsync(string sessionId)
        {
            var session = await LoadSessionAsync(sessionId);
            if (session == null)
                return new { message = "Session not found." };

            return new
            {
                message         = "Interview completed",
                totalQuestions  = session.Questions.Count,
                totalAnswers    = session.Answers.Count
            };
        }

        /// <summary>
        /// Generates a detailed AI evaluation report and persists it to MongoDB.
        /// </summary>
        /// <param name="sessionId">The interview session ID.</param>
        /// <returns>
        /// An object containing the AI evaluation, questions, answers, and recommendations.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if the session is not found or if the AI evaluation call fails.
        /// </exception>
        public async Task<object> GenerateReportAsync(string sessionId)
        {
            var session = await LoadSessionAsync(sessionId);
            if (session == null)
                throw new Exception("Session not found");

            var payload = new
            {
                jd        = session.JobDescription,
                questions = session.Questions.Select(q => q.Text).ToList(),
                answers   = session.Answers.Select(a => a.Transcript ?? string.Empty).ToList()
            };
            var resp = await _httpClient.PostAsJsonAsync("http://localhost:8000/api/evaluate", payload);
            if (!resp.IsSuccessStatusCode)
                throw new Exception("Failed to get evaluation from AI service");

            var aiReport = await resp.Content.ReadFromJsonAsync<JsonElement>();

            var dbReport = new InterviewReport
            {
                Email = session.Email,
                JobDescription = session.JobDescription,
                CandidateFitScore = aiReport.GetProperty("score").GetInt32(),
                Strengths = aiReport.GetProperty("strengths").EnumerateArray().Select(s => s.GetString()!).ToList(),
                ImprovementAreas = aiReport.GetProperty("improvements").EnumerateArray().Select(s => s.GetString()!).ToList(),
                SuggestedFollowUp = aiReport.GetProperty("followUps").EnumerateArray().Select(f => f.GetString()!).ToList(),
                Answers = session.Answers,
                CreatedAt = DateTime.UtcNow 
            };
            await _dbContext.Reports.InsertOneAsync(dbReport);

            return new
            {
                jd          = aiReport.GetProperty("jd").GetString(),
                score       = dbReport.CandidateFitScore,
                questions   = aiReport.GetProperty("questions").EnumerateArray().Select(q => q.GetString()!).ToList(),
                answers     = session.Answers.Select(a => new { question = a.Question, transcript = a.Transcript, audio = a.AudioUrl }).ToList(),
                strengths   = dbReport.Strengths,
                improvements= dbReport.ImprovementAreas,
                followUps   = dbReport.SuggestedFollowUp
            };
        }

        /// <summary>
        /// Retrieves all reports for a given email address.
        /// </summary>
        /// <param name="email">Candidate's email address.</param>
        /// <returns>List of <see cref="InterviewReport"/> for the specified email.</returns>
        public async Task<List<InterviewReport>> GetReportsByEmailAsync(string email) =>
            await _dbContext.Reports.Find(r => r.Email == email.Trim().ToLowerInvariant()).ToListAsync();

        /// <summary>
        /// Retrieves a single report by its ID.
        /// </summary>
        /// <param name="id">The report ID.</param>
        /// <returns>
        /// The <see cref="InterviewReport"/> if found; otherwise null.
        /// </returns>
        public async Task<InterviewReport?> GetReportByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var oid)) return null;
            return await _dbContext.Reports.Find(r => r.Id == id).FirstOrDefaultAsync();
        }

        #region Helpers

        /// <summary>
        /// Loads an interview session document from MongoDB by session ID.
        /// </summary>
        /// <param name="sessionId">The session ID.</param>
        /// <returns>
        /// The <see cref="InterviewSession"/> if found; otherwise, null.
        /// </returns>
        private async Task<InterviewSession?> LoadSessionAsync(string sessionId)
        {
            return await _dbContext.Sessions.Find(s => s.Id == sessionId).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Saves updates to an interview session document in MongoDB.
        /// </summary>
        /// <param name="session">The interview session object to save.</param>
        private async Task SaveSessionAsync(InterviewSession session)
        {
            var filter = Builders<InterviewSession>.Filter.Eq(s => s.Id, session.Id);
            await _dbContext.Sessions.ReplaceOneAsync(filter, session);
        }

        #endregion
    }
}
