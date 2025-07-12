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
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("_id")]
        public string Id { get; set; } = "";

        [Required]
        [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
        public string Email { get; set; } = "";

        [Required]
        public string JobDescription { get; set; } = "";

        [Range(0, 100, ErrorMessage = "CandidateFitScore must be between 0 and 100.")]
        public int CandidateFitScore { get; set; }

        public List<string> Strengths { get; set; } = new();
        public List<string> ImprovementAreas { get; set; } = new();
        public List<string> SuggestedFollowUp { get; set; } = new();
        public List<Answer> Answers { get; set; } = new();
    }
}
