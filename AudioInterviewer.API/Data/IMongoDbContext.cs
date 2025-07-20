using AudioInterviewer.API.Models;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System.Collections.Generic;

namespace AudioInterviewer.API.Data
{
    public interface IMongoDbContext
    {
        IMongoCollection<InterviewSession> Sessions { get; }
        IMongoCollection<InterviewReport> Reports { get; }
        IGridFSBucket GridFsBucket { get; }
    }
}
