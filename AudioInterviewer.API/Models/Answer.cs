using System.ComponentModel.DataAnnotations;

namespace AudioInterviewer.API.Models
{
    /// <summary>
    /// Represents a candidate's answer with audio and transcript.
    /// </summary>
    public class Answer
    {
        [Required]
        public string Question { get; set; } = "";

        [Required]
        [Url(ErrorMessage = "AudioUrl must be a valid URL.")]
        public string AudioUrl { get; set; } = "";

        [Required]
        [Url(ErrorMessage = "AudioUrl must be a valid URL.")]
        public string Transcript { get; set; } = "";
    }
}
