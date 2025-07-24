using System;
using System.Threading.Tasks;
using AudioInterviewer.API.Controllers;
using AudioInterviewer.API.Data;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using Xunit;

/// <summary>
/// Contains unit tests for <see cref="AudioController"/>, focusing on audio retrieval error handling.
/// </summary>
public class AudioControllerTests
{
    private readonly Mock<IGridFSBucket> _mockGridFsBucket;
    private readonly Mock<IMongoDbContext> _mockDbContext;
    private readonly AudioController _controller;

    /// <summary>
    /// Initializes a new instance of <see cref="AudioControllerTests"/> and sets up mocks.
    /// </summary>
    public AudioControllerTests()
    {
        _mockGridFsBucket = new Mock<IGridFSBucket>();
        _mockDbContext = new Mock<IMongoDbContext>();
        _mockDbContext.SetupGet(x => x.GridFsBucket).Returns(_mockGridFsBucket.Object);

        _controller = new AudioController(_mockDbContext.Object);
    }

    /// <summary>
    /// Tests that <see cref="AudioController.GetAudio(string)"/> returns <see cref="NotFoundObjectResult"/>
    /// when the audio file is not found in the database.
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
        Assert.Equal("Audio file not found in database.", notFound.Value);
    }

    /// <summary>
    /// Tests that <see cref="AudioController.GetAudio(string)"/> returns a <see cref="ObjectResult"/>
    /// with status code 500 when an unexpected exception occurs.
    /// </summary>
    [Fact]
    public async Task GetAudio_ReturnsServerError_WhenOtherErrorOccurs()
    {
        var fileId = ObjectId.GenerateNewId();

        _mockGridFsBucket
            .Setup(x => x.OpenDownloadStreamAsync(fileId, null, default))
            .ThrowsAsync(new Exception("Unexpected"));

        var result = await _controller.GetAudio(fileId.ToString());

        var serverError = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, serverError.StatusCode);
        Assert.StartsWith("Error retrieving audio:", serverError.Value!.ToString());
    }
}
