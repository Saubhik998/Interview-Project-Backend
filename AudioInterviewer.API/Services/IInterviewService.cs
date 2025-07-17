using AudioInterviewer.API.Models;

namespace AudioInterviewer.API.Services
{
    public interface IInterviewService
    {
        Task InitializeSessionAsync(string jobDescription, string email);
        Task<string?> GetNextQuestionAsync();
        Task<bool> SubmitAnswerAsync(AnswerDto answerDto);
        List<Question> GetQuestions();
        int CurrentIndex { get; }
        Task<object> GetCompletionSummaryAsync();
        Task<object> GenerateReportAsync();
        Task<List<InterviewReport>> GetReportsByEmailAsync(string email);
        Task<InterviewReport?> GetReportByIdAsync(string id);
    }
}
