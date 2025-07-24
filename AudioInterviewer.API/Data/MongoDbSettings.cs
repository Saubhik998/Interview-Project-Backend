namespace AudioInterviewer.API.Data
{
    /// <summary>
    /// Represents configuration settings required to connect to a MongoDB database.
    /// </summary>
    public class MongoDbSettings
    {
        /// <summary>
        /// Gets or sets the connection string used to connect to the MongoDB instance.
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the MongoDB database to be used.
        /// </summary>
        public string DatabaseName { get; set; } = string.Empty;
    }
}
