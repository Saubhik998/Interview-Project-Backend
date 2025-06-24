using AudioInterviewer.API.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace AudioInterviewer.API.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<InterviewSession> Sessions => _database.GetCollection<InterviewSession>("InterviewSessions");
        public IMongoCollection<InterviewReport> Reports => _database.GetCollection<InterviewReport>("InterviewReports");
    }
}
