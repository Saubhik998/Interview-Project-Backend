using AudioInterviewer.API.Models;

namespace AudioInterviewer.API.Services.External
{
    /// <summary>
    /// Provides an abstraction for interacting with an external API
    /// to generate interview questions based on job description and previous answers.
    /// </summary>
    public interface IApiClient
    {
        /// <summary>
        /// Retrieves the initial interview question based on the provided job description.
        /// </summary>
        /// <param name="jd">The job description for which to generate the first question.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation,
        /// with the first generated question as a string.
        /// </returns>
        Task<string> GetFirstQuestionAsync(string jd);

        /// <summary>
        /// Retrieves the next interview question based on job description,
        /// the previous question, and the candidate's answer.
        /// </summary>
        /// <param name="jd">The job description for the interview.</param>
        /// <param name="previousQuestion">The previous question that was asked.</param>
        /// <param name="answer">The candidate's answer to the previous question.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation,
        /// with the next generated question as a string.
        /// </returns>
        Task<string> GetNextQuestionAsync(string jd, string previousQuestion, string answer);
    }
}
