using Xunit;
using System;
using AudioInterviewer.API.Data;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace AudioInterviewer.UnitTests.Data
{
    public class MongoDbSettingsTests
    {
        [Fact]
        public void MongoDbSettings_Defaults_AreEmptyStrings()
        {
            var settings = new MongoDbSettings();
            Assert.Equal(string.Empty, settings.ConnectionString);
            Assert.Equal(string.Empty, settings.DatabaseName);
        }

        [Theory]
        [InlineData("mongodb://localhost:27017", "TestDb")]
        [InlineData("mongodb://server:27018", "ProdDb")]
        public void MongoDbSettings_CanSetProperties(string conn, string db)
        {
            var settings = new MongoDbSettings
            {
                ConnectionString = conn,
                DatabaseName = db
            };

            Assert.Equal(conn, settings.ConnectionString);
            Assert.Equal(db, settings.DatabaseName);
        }
    }

    public class MongoDbContextTests
    {
        [Fact]
        public void MongoDbContext_CreatesGridFsBucket_And_Collections()
        {
            // Arrange: create dummy settings
            var mongoSettings = Options.Create(new MongoDbSettings
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "AudioTestDb"
            });

            // Act
            var context = new MongoDbContext(mongoSettings);

            // Assert
            Assert.NotNull(context.GridFsBucket);
            Assert.NotNull(context.Sessions);
            Assert.NotNull(context.Reports);
            Assert.Equal("InterviewSessions", context.Sessions.CollectionNamespace.CollectionName);
            Assert.Equal("InterviewReports", context.Reports.CollectionNamespace.CollectionName);
        }

        [Fact]
        public void MongoDbContext_NullSettings_Throws()
        {
            // As per Option 2: expect the actual thrown exception
            Assert.Throws<NullReferenceException>(() => new MongoDbContext(null!));
        }

        [Fact]
        public void MongoDbContext_NullConnectionString_Throws()
        {
            var settings = Options.Create(new MongoDbSettings
            {
                ConnectionString = null!,
                DatabaseName = "AudioTestDb"
            });
            Assert.Throws<ArgumentNullException>(() => new MongoDbContext(settings));
        }

        [Fact]
        public void MongoDbContext_NullDatabaseName_Throws()
        {
            var settings = Options.Create(new MongoDbSettings
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = null!
            });
            Assert.Throws<ArgumentNullException>(() => new MongoDbContext(settings));
        }
    }
}
