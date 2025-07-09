using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace AudioInterviewer.API.Models
{
    /// <summary>
    /// Represents a complete interview session.
    /// </summary>
    public class InterviewSession
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("email")] 
        public string Email { get; set; } = "";

        [BsonElement("jobDescription")]
        public string JobDescription { get; set; } = "";

        [BsonElement("questions")]
        public List<Question> Questions { get; set; } = new();

        [BsonElement("answers")]
        public List<Answer> Answers { get; set; } = new();

        [BsonElement("currentIndex")]
        public int CurrentIndex { get; set; } = 0;
    }
}
