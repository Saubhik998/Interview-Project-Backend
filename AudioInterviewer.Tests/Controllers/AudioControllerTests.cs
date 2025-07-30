using System;
using System.Threading.Tasks;
using AudioInterviewer.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using Xunit;

/// <summary>
/// Contains unit tests for AudioController, focusing on audio retrieval error handling.
/// </summary>
public class AudioControllerTests
{
    private readonly Mock<IGridFSBucket> _mockGridFsBucket;
    private readonly AudioController _controller;

    /// <summary>
    /// Initializes a new instance of AudioControllerTests and sets up mocks.
    /// </summary>
    public AudioControllerTests()
    {
        _mockGridFsBucket = new Mock<IGridFSBucket>();
        _controller = new AudioController(_mockGridFsBucket.Object);
    }

    /// <summary>
    /// Tests that GetAudio returns NotFound when the audio file is not found in the database.
    /// </summary>
    [Fact]
    public async Task GetAudio_ReturnsNotFound_WhenFileNotInDatabase()
    {
        var fileId = ObjectId.GenerateNewId();

        _mockGridFsBucket
            .Setup(x => x.OpenDownloadStreamAsync(fileId, null, default))
            .ThrowsAsync(new GridFSFileNotFoundException("not found"));

        var result = await _controller.GetAudio(fileId.ToString());
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Audio file not found.", notFound.Value);
    }

    /// <summary>
    /// Tests that GetAudio returns BadRequest when an unexpected exception occurs.
    /// </summary>
    [Fact]
    public async Task GetAudio_ReturnsBadRequest_WhenOtherErrorOccurs()
    {
        var fileId = ObjectId.GenerateNewId();

        _mockGridFsBucket
            .Setup(x => x.OpenDownloadStreamAsync(fileId, null, default))
            .ThrowsAsync(new Exception("Unexpected"));

        var result = await _controller.GetAudio(fileId.ToString());
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.StartsWith("Error retrieving audio:", badRequest.Value!.ToString());
    }

    /// <summary>
    /// Tests that GetAudio returns BadRequest for invalid ObjectId.
    /// </summary>
    [Fact]
    public async Task GetAudio_ReturnsBadRequest_WhenIdIsInvalid()
    {
        var result = await _controller.GetAudio("not-a-valid-object-id");
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid audio file id.", badRequest.Value);
    }
}
