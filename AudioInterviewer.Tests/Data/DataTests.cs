using Xunit;
using System;
using AudioInterviewer.API.Data;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace AudioInterviewer.UnitTests.Data
{
    /// <summary>
    /// Unit tests for <see cref="MongoDbSettings"/>, verifying default values and property assignment.
    /// </summary>
    public class MongoDbSettingsTests
    {
        /// <summary>
        /// Verifies that the default constructor of <see cref="MongoDbSettings"/>
        /// initializes <c>ConnectionString</c> and <c>DatabaseName</c> to empty strings.
        /// </summary>
        [Fact]
        public void MongoDbSettings_Defaults_AreEmptyStrings()
        {
            var settings = new MongoDbSettings();
            Assert.Equal(string.Empty, settings.ConnectionString);
            Assert.Equal(string.Empty, settings.DatabaseName);
        }

        /// <summary>
        /// Verifies that the <see cref="MongoDbSettings"/> properties can be set and retrieved.
        /// </summary>
        /// <param name="conn">The MongoDB connection string to set.</param>
        /// <param name="db">The MongoDB database name to set.</param>
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

    /// <summary>
    /// Unit tests for <see cref="MongoDbContext"/>, verifying initialization and property validation.
    /// </summary>
    public class MongoDbContextTests
    {
        /// <summary>
        /// Verifies that <see cref="MongoDbContext"/> creates the GridFS bucket and collections with correct names.
        /// </summary>
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

        /// <summary>
        /// Ensures that a <see cref="NullReferenceException"/> is thrown if the <see cref="IOptions{MongoDbSettings}"/> is null.
        /// </summary>
        [Fact]
        public void MongoDbContext_NullSettings_Throws()
        {
            Assert.Throws<NullReferenceException>(() => new MongoDbContext(null!));
        }

        /// <summary>
        /// Ensures that an <see cref="ArgumentNullException"/> is thrown if <c>ConnectionString</c> is null.
        /// </summary>
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

        /// <summary>
        /// Ensures that an <see cref="ArgumentNullException"/> is thrown if <c>DatabaseName</c> is null.
        /// </summary>
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
