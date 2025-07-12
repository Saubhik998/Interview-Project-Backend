using AudioInterviewer.API.Data;
using AudioInterviewer.API.Models;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Microsoft.Extensions.Options;
using Moq;
namespace AudioInterviewer.Tests.Data
{
    public class TestMongoDbContext : MongoDbContext
    {
        public new IMongoCollection<InterviewSession> Sessions { get; set; }
        public new IMongoCollection<InterviewReport> Reports { get; set; }
        public new IGridFSBucket GridFsBucket { get; set; }

        public TestMongoDbContext(IOptions<MongoDbSettings> settings) : base(settings)
        {
            Sessions = new Mock<IMongoCollection<InterviewSession>>().Object;
            Reports = new Mock<IMongoCollection<InterviewReport>>().Object;
            GridFsBucket = new Mock<IGridFSBucket>().Object;
        }
    }
}
