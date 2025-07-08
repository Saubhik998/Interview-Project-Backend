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
        [BsonId] // Marks this property as the MongoDB document IDA
        [BsonRepresentation(BsonType.ObjectId)] // Allows passing it as a string instead of ObjectId
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public string JobDescription { get; set; } = "";
        public List<Question> Questions { get; set; } = new();
        public List<Answer> Answers { get; set; } = new();
        public int CurrentIndex { get; set; } = 0;
    }
}
