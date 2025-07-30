namespace AudioInterviewer.API.Models
{
    /// <summary>
    /// DTO for interview initialization response.
    /// </summary>
    public class InitResponse
    {
        public string Message { get; set; } = "";
        public string SessionId { get; set; } = "";
        public string FirstQuestion { get; set; } = "";
    }
}
