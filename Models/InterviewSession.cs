using System.Collections.Generic;

namespace AudioInterviewer.API.Models
{
    /// <summary>
    /// Represents a complete interview session.
    /// </summary>
    public class InterviewSession
    {
        public string JobDescription { get; set; } = "";
        public List<Question> Questions { get; set; } = new();
        public List<Answer> Answers { get; set; } = new();
        public int CurrentIndex { get; set; } = 0;
    }
}
