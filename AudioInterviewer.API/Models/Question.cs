using System.ComponentModel.DataAnnotations;

namespace AudioInterviewer.API.Models
{
    /// <summary>
    /// Represents a single interview question.
    /// </summary>
    public class Question
    {
        [Required]
        [StringLength(300, ErrorMessage = "Question text is too long.")]
        public string Text { get; set; } = "";
    }
}
