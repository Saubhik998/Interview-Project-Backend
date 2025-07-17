using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AudioInterviewer.API.Models
{
    /// <summary>
    /// Represents a complete interview session.
    /// </summary>
    public class InterviewSession
    {
        /// <summary>
        /// Unique identifier for the session.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        /// <summary>
        /// Candidate's email address.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [BsonElement("email")]
        public string Email { get; set; } = "";

        /// <summary>
        /// Job description provided by the candidate.
        /// </summary>
        [Required(ErrorMessage = "Job description is required.")]
        [BsonElement("jobDescription")]
        public string JobDescription { get; set; } = "";

        /// <summary>
        /// List of interview questions generated.
        /// </summary>
        [BsonElement("questions")]
        public List<Question> Questions { get; set; } = new();

        /// <summary>
        /// List of answers provided by the candidate.
        /// </summary>
        [BsonElement("answers")]
        public List<Answer> Answers { get; set; } = new();

        /// <summary>
        /// Index of the current question in the session.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Current index cannot be negative.")]
        [BsonElement("currentIndex")]
        public int CurrentIndex { get; set; } = 0;
    }
}
