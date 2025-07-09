using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

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

        public string Email { get; set; } = "";
        public string JobDescription { get; set; } = "";
        public int CandidateFitScore { get; set; }
        public List<string> Strengths { get; set; } = new();
        public List<string> ImprovementAreas { get; set; } = new();
        public List<string> SuggestedFollowUp { get; set; } = new();
        public List<Answer> Answers { get; set; } = new();
    }
}
