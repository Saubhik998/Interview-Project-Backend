namespace AudioInterviewer.API.Models
{
    /// <summary>
    /// Data Transfer Object for receiving candidate's answer.
    /// </summary>
    public class AnswerDto
    {
        public string Question { get; set; } = "";
        /// <summary>
        /// Base64-encoded audio data from frontend.
        /// </summary>
        public string AudioBase64 { get; set; } = "";

        /// <summary>
        /// Text transcription of the spoken answer.
        /// </summary>
        public string Transcript { get; set; } = "";
    }
}
