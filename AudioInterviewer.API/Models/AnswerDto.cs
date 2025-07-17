using System.ComponentModel.DataAnnotations;

namespace AudioInterviewer.API.Models
{
    /// <summary>
    /// Data Transfer Object for receiving candidate's answer.
    /// </summary>
    public class AnswerDto
    {
        /// <summary>
        /// The question being answered.
        /// </summary>
        [Required(ErrorMessage = "Question is required.")]
        [MinLength(10, ErrorMessage = "Question text is too short.")]
        public string Question { get; set; } = "";

        /// <summary>
        /// Base64-encoded audio data from frontend.
        /// </summary>
        [Required(ErrorMessage = "Audio data is required.")]
        [MinLength(5000, ErrorMessage = "Audio data is too short or malformed.")]
        public string AudioBase64 { get; set; } = "";

        /// <summary>
        /// Text transcription of the spoken answer.
        /// </summary>
        [Required(ErrorMessage = "Transcript is required.")]
        [StringLength(1000, ErrorMessage = "Transcript is too long.")]
        public string Transcript { get; set; } = "";
    }
}
