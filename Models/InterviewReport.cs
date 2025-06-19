using System.Collections.Generic;

namespace AudioInterviewer.API.Models
{
    /// <summary>
    /// Represents the final report generated at the end of the interview.
    /// </summary>
    public class InterviewReport
    {
        public int CandidateFitScore { get; set; }
        public List<string> Strengths { get; set; } = new();
        public List<string> ImprovementAreas { get; set; } = new();
        public string SuggestedFollowUp { get; set; } = "";
        public List<Answer> Answers { get; set; } = new();
    }
}
