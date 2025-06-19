namespace AudioInterviewer.API.Models
{
    /// <summary>
    /// Data Transfer Object for receiving candidate's answer.
    /// </summary>
    public class AnswerDto
    {
        /// <summary>
        /// URL to the recorded audio file (frontend uploads audio).
        /// </summary>
        public string AudioUrl { get; set; } = "";

        /// <summary>
        /// Text transcription of the spoken answer.
        /// </summary>
        public string Transcript { get; set; } = "";
    }
}
