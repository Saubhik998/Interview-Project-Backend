namespace AudioInterviewer.API.Models
{
    /// <summary>
    /// Represents a candidate's answer with audio and transcript.
    /// </summary>
    public class Answer
    {
        public string Question { get; set; } = "";
        public string AudioUrl { get; set; } = "";
        public string Transcript { get; set; } = "";
    }
}
