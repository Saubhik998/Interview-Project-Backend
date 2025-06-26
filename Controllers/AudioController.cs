using AudioInterviewer.API.Data;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;

namespace AudioInterviewer.API.Controllers
{
    [ApiController]
    [Route("api/audio")]
    public class AudioController : ControllerBase
    {
        private readonly MongoDbContext _dbContext;

        public AudioController(MongoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

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
