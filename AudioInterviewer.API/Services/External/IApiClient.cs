using AudioInterviewer.API.Models;

namespace AudioInterviewer.API.Services.External
{
    public interface IApiClient
    {
        Task<string> GetFirstQuestionAsync(string jd);
        Task<string> GetNextQuestionAsync(string jd, string previousQuestion, string answer);
    }
}
