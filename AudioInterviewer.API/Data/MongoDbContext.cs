using AudioInterviewer.API.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace AudioInterviewer.API.Data
{   
    public class MongoDbContext : IMongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);

            // Initialize GridFS
            GridFsBucket = new GridFSBucket(_database, new GridFSBucketOptions
            {
                BucketName = "audioFiles",
                ChunkSizeBytes = 255 * 1024, // 255 KB chunks
                WriteConcern = WriteConcern.WMajority,
                ReadPreference = ReadPreference.Primary
            });
        }

        public IMongoCollection<InterviewSession> Sessions => _database.GetCollection<InterviewSession>("InterviewSessions");
        public IMongoCollection<InterviewReport> Reports => _database.GetCollection<InterviewReport>("InterviewReports");

        // GridFS bucket for audio files
        public IGridFSBucket GridFsBucket { get; }
    }
}
