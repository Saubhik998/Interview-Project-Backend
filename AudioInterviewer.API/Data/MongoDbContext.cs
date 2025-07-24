using AudioInterviewer.API.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace AudioInterviewer.API.Data
{
    /// <summary>
    /// Implementation of <see cref="IMongoDbContext"/> providing MongoDB database access,
    /// collections for sessions and reports, and GridFS bucket for audio files.
    /// </summary>
    public class MongoDbContext : IMongoDbContext
    {
        private readonly IMongoDatabase _database;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbContext"/> class using the provided settings.
        /// </summary>
        /// <param name="settings">MongoDB settings containing connection string and database name.</param>
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

        /// <summary>
        /// Gets the MongoDB collection containing interview session documents.
        /// </summary>
        public IMongoCollection<InterviewSession> Sessions => _database.GetCollection<InterviewSession>("InterviewSessions");

        /// <summary>
        /// Gets the MongoDB collection containing interview report documents.
        /// </summary>
        public IMongoCollection<InterviewReport> Reports => _database.GetCollection<InterviewReport>("InterviewReports");

        /// <summary>
        /// Gets the GridFS bucket used for storing and retrieving audio files.
        /// </summary>
        public IGridFSBucket GridFsBucket { get; }
    }
}
