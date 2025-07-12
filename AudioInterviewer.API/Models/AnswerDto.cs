using System.ComponentModel.DataAnnotations;

namespace AudioInterviewer.API.Models
{
    /// <summary>
    /// Data Transfer Object for receiving candidate's answer.
    /// </summary>
    public class AnswerDto
    {
        [Required]
        public string Question { get; set; } = "";

        /// <summary>
        /// Base64-encoded audio data from frontend.
        /// </summary>
        [Required]
        public string AudioBase64 { get; set; } = "";

        /// <summary>
        /// Text transcription of the spoken answer.
        /// </summary>
        [Required]
        [StringLength(1000, ErrorMessage = "Transcript is too long.")]
        public string Transcript { get; set; } = "";
    }
}
