using AudioInterviewer.API.Models;

namespace AudioInterviewer.API.Services
{
    public interface IInterviewService
    {
        Task InitializeSessionAsync(string jobDescription, string email);
        Task<string?> GetNextQuestionAsync();
        bool SubmitAnswer(AnswerDto answerDto);
        List<Question> GetQuestions();
        int CurrentIndex { get; }
        object GetCompletionSummary();
        Task<object> GenerateReportAsync();
        Task<List<InterviewReport>> GetReportsByEmailAsync(string email);
        Task<InterviewReport?> GetReportByIdAsync(string id);
    }
}
