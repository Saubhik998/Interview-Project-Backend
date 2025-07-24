using AudioInterviewer.API.Models;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System.Collections.Generic;

namespace AudioInterviewer.API.Data
{
    /// <summary>
    /// Provides the abstraction for MongoDB context used within the AudioInterviewer API,
    /// including collections and GridFS bucket accessors.
    /// </summary>
    public interface IMongoDbContext
    {
        /// <summary>
        /// Gets the MongoDB collection containing interview session documents.
        /// </summary>
        IMongoCollection<InterviewSession> Sessions { get; }

        /// <summary>
        /// Gets the MongoDB collection containing interview report documents.
        /// </summary>
        IMongoCollection<InterviewReport> Reports { get; }

        /// <summary>
        /// Gets the GridFS bucket used for storing and retrieving audio files.
        /// </summary>
        IGridFSBucket GridFsBucket { get; }
    }
}
