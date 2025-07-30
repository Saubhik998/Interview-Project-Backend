using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AudioInterviewer.API.Controllers
{
    /// <summary>
    /// Controller for managing audio files stored in MongoDB GridFS.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AudioController : ControllerBase
    {
        private readonly IGridFSBucket _gridFsBucket;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioController"/> class.
        /// </summary>
        /// <param name="gridFsBucket">MongoDB GridFS bucket for file storage.</param>
        public AudioController(IGridFSBucket gridFsBucket)
        {
            _gridFsBucket = gridFsBucket;
        }

        /// <summary>
        /// Retrieves an audio file by its id.
        /// </summary>
        /// <param name="id">The ObjectId string of the audio file.</param>
        /// <returns>Audio file stream if found.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(FileStreamResult), 200)]
        [ProducesResponseType(typeof(ObjectResult), 400)]
        [ProducesResponseType(typeof(ObjectResult), 404)]
        public async Task<IActionResult> GetAudio(string id)
        {
            if (!ObjectId.TryParse(id, out var fileId))
                return BadRequest("Invalid audio file id.");

            try
            {
                var stream = await _gridFsBucket.OpenDownloadStreamAsync(fileId);
                return File(stream, "audio/mpeg"); // Adjust MIME type if necessary
            }
            catch (GridFSFileNotFoundException)
            {
                return NotFound("Audio file not found.");
            }
            catch (Exception ex)
            {
                // Return BadRequest on unexpected errors per your request
                return BadRequest($"Error retrieving audio: {ex.Message}");
            }
        }

        /// <summary>
        /// Uploads a new audio file.
        /// </summary>
        /// <param name="audioFile">The audio file stream from the form-data.</param>
        /// <returns>The ObjectId string of the stored file.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(string), 201)]
        [ProducesResponseType(typeof(ObjectResult), 400)]
        public async Task<IActionResult> UploadAudio([FromForm] IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
                return BadRequest("No audio file provided.");

            try
            {
                using var stream = audioFile.OpenReadStream();
                var fileId = await _gridFsBucket.UploadFromStreamAsync(audioFile.FileName, stream);
                return CreatedAtAction(nameof(GetAudio), new { id = fileId.ToString() }, fileId.ToString());
            }
            catch (Exception ex)
            {
                return BadRequest($"Error uploading audio: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes an audio file by id.
        /// </summary>
        /// <param name="id">The ObjectId string of the audio file to delete.</param>
        /// <returns>Status of the deletion.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)] // No Content
        [ProducesResponseType(typeof(ObjectResult), 400)]
        [ProducesResponseType(typeof(ObjectResult), 404)]
        public async Task<IActionResult> DeleteAudio(string id)
        {
            if (!ObjectId.TryParse(id, out var fileId))
                return BadRequest("Invalid audio file id.");

            try
            {
                await _gridFsBucket.DeleteAsync(fileId);
                return NoContent(); // 204
            }
            catch (GridFSFileNotFoundException)
            {
                return NotFound("Audio file to delete not found.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting audio: {ex.Message}");
            }
        }
    }
}
