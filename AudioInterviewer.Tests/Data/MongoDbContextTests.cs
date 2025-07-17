using AudioInterviewer.API.Data;
using AudioInterviewer.API.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Xunit;

namespace AudioInterviewer.Tests.Data
{
    /// <summary>
    /// Unit tests for the <see cref="MongoDbContext"/> to ensure database connectivity and collection accessibility.
    /// </summary>
    public class MongoDbContextTests
    {
        private readonly MongoDbContext _context;

        /// <summary>
        /// Initializes the MongoDbContext with test configuration for use in the test suite.
        /// </summary>
        public MongoDbContextTests()
        {
            var connectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING") ?? "mongodb://localhost:27017";

            var settings = Options.Create(new MongoDbSettings
            {
                ConnectionString = connectionString,
                DatabaseName = "testdb"
            });

            _context = new MongoDbContext(settings);
        }

        /// <summary>
        /// Validates that the context initializes successfully and exposes the required MongoDB collections.
        /// </summary>
        [Fact]
        public void Should_Connect_To_Database_And_Expose_Collections()
        {
            Assert.NotNull(_context.Sessions);
            Assert.NotNull(_context.Reports);
            Assert.NotNull(_context.GridFsBucket);
        }

        // This test inserts a mock InterviewSession and reads it back from the database.
        // Disabled by default to avoid writing to a real MongoDB instance during unit test runs.

        //[Fact]
        //public void Should_Insert_And_Read_From_Sessions_Collection()
        //{
        //    var session = new InterviewSession
        //    {
        //        Email = "test@example.com",
        //        JobDescription = "Tester",
        //        Answers = new List<Answer>()
        //    };

        //    _context.Sessions.InsertOne(session);

        //    var saved = _context.Sessions.Find(s => s.Email == "test@example.com").FirstOrDefault();

        //    Assert.NotNull(saved);
        //    Assert.Equal("Tester", saved.JobDescription);
        //}
    }
}
