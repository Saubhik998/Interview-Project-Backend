using System.ComponentModel.DataAnnotations;

namespace AudioInterviewer.API.Models
{
    /// <summary>
    /// Represents a single interview question.
    /// </summary>
    public class Question
    {
        /// <summary>
        /// The text content of the interview question.
        /// </summary>
        [Required(ErrorMessage = "Question text is required.")]
        [StringLength(300, ErrorMessage = "Question text is too long.")]
        public string Text { get; set; } = "";
    }
}
