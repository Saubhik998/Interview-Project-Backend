using System.ComponentModel.DataAnnotations;

namespace AudioInterviewer.API.Models
{
    /// <summary>
    /// Data Transfer Object for receiving candidate's answer.
    /// </summary>
    public class AnswerDto
    {
        /// <summary>
        /// The unique interview session identifier.
        /// </summary>
        [Required(ErrorMessage = "SessionId is required.")]
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// The question being answered.
        /// </summary>
        [Required(ErrorMessage = "Question is required.")]
        [MinLength(10, ErrorMessage = "Question text is too short.")]
        public string Question { get; set; } = "";

        /// <summary>
        /// Base64-encoded audio data from frontend.
        /// </summary>
        
        [MinLength(5000, ErrorMessage = "Audio data is too short or malformed.")]
        public string AudioBase64 { get; set; } = "";

        /// <summary>
        /// Text transcription of the spoken answer.
        /// </summary>
        
        [StringLength(5000, ErrorMessage = "Transcript is too long.")]
        public string Transcript { get; set; } = "";
    }
}
