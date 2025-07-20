using AudioInterviewer.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AudioInterviewer.API.Services
{
    public interface IInterviewService
    {
        /// <summary>
        /// Initializes a new interview session and returns the session ID.
        /// </summary>
        Task<string> InitializeSessionAsync(string jobDescription, string email);

        /// <summary>
        /// Retrieves the next question for the given session.
        /// </summary>
        Task<string?> GetNextQuestionAsync(string sessionId);

        /// <summary>
        /// Submits the candidate's answer for the given session.
        /// </summary>
        Task<bool> SubmitAnswerAsync(string sessionId, AnswerDto answerDto);

        /// <summary>
        /// Gets the list of questions for the given session.
        /// </summary>
        List<Question> GetQuestions(string sessionId);

        /// <summary>
        /// Gets the current question index for the given session.
        /// </summary>
        int CurrentIndex(string sessionId);

        /// <summary>
        /// Returns a summary after interview completion for the given session.
        /// </summary>
        Task<object> GetCompletionSummaryAsync(string sessionId);

        /// <summary>
        /// Generates and returns a detailed interview report after evaluation for the given session.
        /// </summary>
        Task<object> GenerateReportAsync(string sessionId);

        /// <summary>
        /// Retrieves all interview reports by candidate email.
        /// </summary>
        Task<List<InterviewReport>> GetReportsByEmailAsync(string email);

        /// <summary>
        /// Retrieves a specific interview report by its MongoDB ObjectId.
        /// </summary>
        Task<InterviewReport?> GetReportByIdAsync(string id);
    }
}
