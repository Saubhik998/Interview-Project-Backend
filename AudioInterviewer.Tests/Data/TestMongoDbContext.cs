using AudioInterviewer.API.Data;
using AudioInterviewer.API.Models;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Microsoft.Extensions.Options;
using Moq;

namespace AudioInterviewer.Tests.Data
{
    /// <summary>
    /// Mock implementation of <see cref="MongoDbContext"/> used specifically for testing services
    /// without requiring a live MongoDB connection.
    /// </summary>
    public class TestMongoDbContext : MongoDbContext
    {
        /// <summary>
        /// Gets or sets the mocked collection of <see cref="InterviewSession"/> documents.
        /// </summary>
        public new IMongoCollection<InterviewSession> Sessions { get; set; }

        /// <summary>
        /// Gets or sets the mocked collection of <see cref="InterviewReport"/> documents.
        /// </summary>
        public new IMongoCollection<InterviewReport> Reports { get; set; }

        /// <summary>
        /// Gets or sets the mocked GridFS bucket used for simulating audio file storage.
        /// </summary>
        public new IGridFSBucket GridFsBucket { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestMongoDbContext"/> class using mock objects.
        /// </summary>
        /// <param name="settings">MongoDB settings provided via dependency injection.</param>
        public TestMongoDbContext(IOptions<MongoDbSettings> settings) : base(settings)
        {
            Sessions = new Mock<IMongoCollection<InterviewSession>>().Object;
            Reports = new Mock<IMongoCollection<InterviewReport>>().Object;
            GridFsBucket = new Mock<IGridFSBucket>().Object;
        }
    }
}
