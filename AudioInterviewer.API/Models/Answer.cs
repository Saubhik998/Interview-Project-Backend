using System.ComponentModel.DataAnnotations;

namespace AudioInterviewer.API.Models
{
    /// <summary>
    /// Represents a candidate's answer with audio and transcript.
    /// </summary>
    public class Answer
    {
        /// <summary>
        /// The interview question that was answered.
        /// </summary>
        [Required(ErrorMessage = "Question is required.")]
        public string Question { get; set; } = "";

        /// <summary>
        /// A valid URL pointing to the audio file.
        /// </summary>
        [Required(ErrorMessage = "AudioUrl is required.")]
        [Url(ErrorMessage = "AudioUrl must be a valid URL.")]
        public string AudioUrl { get; set; } = "";

        /// <summary>
        /// The transcript text of the answer.
        /// </summary>
        [Required(ErrorMessage = "Transcript is required.")]
        public string Transcript { get; set; } = "";
    }
}
