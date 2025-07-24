using AudioInterviewer.API.Data;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;

namespace AudioInterviewer.API.Controllers
{
    /// <summary>
    /// Controller for handling audio file retrieval operations.
    /// </summary>
    [ApiController]
    [Route("api/audio")]
    public class AudioController : ControllerBase
    {
        private readonly IMongoDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioController"/> class.
        /// </summary>
        /// <param name="dbContext">The MongoDB context used to access the GridFS bucket.</param>
        public AudioController(IMongoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves an audio file from the database by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier (ObjectId) of the audio file in GridFS.</param>
        /// <returns>
        /// Returns the audio file as a file result with content type "audio/webm" if found;
        /// otherwise, returns a 404 Not Found or 500 Internal Server Error with a relevant message.
        /// </returns>
        /// <response code="200">Audio file was successfully retrieved.</response>
        /// <response code="404">Audio file not found in the database.</response>
        /// <response code="500">An error occurred while retrieving the audio file.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAudio(string id)
        {
            try
            {
                var fileId = new ObjectId(id);
                var stream = await _dbContext.GridFsBucket.OpenDownloadStreamAsync(fileId);
                return File(stream, "audio/webm");
            }
            catch (GridFSFileNotFoundException)
            {
                return NotFound("Audio file not found in database.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving audio: {ex.Message}");
            }
        }
    }
}
