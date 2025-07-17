using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace AudioInterviewer.API.Models
{
    /// <summary>
    /// Represents the final report generated at the end of the interview.
    /// </summary>
    public class InterviewReport
    {
        /// <summary>
        /// MongoDB object ID for the report.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("_id")]
        public string Id { get; set; } = "";

        /// <summary>
        /// Candidate's email address.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
        public string Email { get; set; } = "";

        /// <summary>
        /// Job description associated with the interview.
        /// </summary>
        [Required(ErrorMessage = "Job description is required.")]
        public string JobDescription { get; set; } = "";

        /// <summary>
        /// AI-evaluated candidate fit score (0–100).
        /// </summary>
        [Range(0, 100, ErrorMessage = "CandidateFitScore must be between 0 and 100.")]
        public int CandidateFitScore { get; set; }

        /// <summary>
        /// List of candidate strengths identified.
        /// </summary>
        public List<string> Strengths { get; set; } = new();

        /// <summary>
        /// List of improvement areas for the candidate.
        /// </summary>
        public List<string> ImprovementAreas { get; set; } = new();

        /// <summary>
        /// Suggested follow-up questions or topics.
        /// </summary>
        public List<string> SuggestedFollowUp { get; set; } = new();

        /// <summary>
        /// Collection of answers submitted by the candidate.
        /// </summary>
        public List<Answer> Answers { get; set; } = new();
    }
}
