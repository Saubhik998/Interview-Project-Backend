using AudioInterviewer.API.Data;
using AudioInterviewer.API.Models;
using Microsoft.Extensions.Options;
using Mongo2Go;
using MongoDB.Driver;
using Xunit;

namespace AudioInterviewer.Tests.Data
{
    public class MongoDbContextTests : IDisposable
    {
        private readonly MongoDbRunner _runner;
        private readonly MongoDbContext _context;

        public MongoDbContextTests()
        {
            _runner = MongoDbRunner.Start();
            var settings = Options.Create(new MongoDbSettings
            {
                ConnectionString = _runner.ConnectionString,
                DatabaseName = "TestDb"
            });

            _context = new MongoDbContext(settings);
        }

        [Fact]
        public void Should_Connect_To_Database_And_Expose_Collections()
        {
            Assert.NotNull(_context.Sessions);
            Assert.NotNull(_context.Reports);
            Assert.NotNull(_context.GridFsBucket);
        }

        [Fact]
        public void Should_Insert_And_Read_From_Sessions_Collection()
        {
            var session = new InterviewSession
            {
                Email = "test@example.com",
                JobDescription = "Tester",
                Answers = new List<Answer>()
            };

            _context.Sessions.InsertOne(session);

            var saved = _context.Sessions.Find(s => s.Email == "test@example.com").FirstOrDefault();

            Assert.NotNull(saved);
            Assert.Equal("Tester", saved.JobDescription);
        }

        public void Dispose()
        {
            _runner.Dispose();
        }
    }
}
