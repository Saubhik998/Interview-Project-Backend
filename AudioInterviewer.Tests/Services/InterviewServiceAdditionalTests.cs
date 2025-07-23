// // Additional unit tests for missing InterviewService methods (fixed IHttpClientFactory setup)

// using System;
// using System.Collections.Generic;
// using System.Text.Json;
// using System.Threading;
// using System.Threading.Tasks;
// using AudioInterviewer.API.Data;
// using AudioInterviewer.API.Models;
// using AudioInterviewer.API.Services;
// using AudioInterviewer.API.Services.External;
// using Moq;
// using MongoDB.Bson;
// using MongoDB.Driver;
// using Xunit;
// using System.Net.Http;

// public class InterviewServiceAdditionalTests
// {
//     private readonly Mock<IMongoDbContext> _mockDbContext = new();
//     private readonly Mock<IApiClient> _mockApiClient = new();
//     private readonly Mock<IHttpClientFactory> _mockHttpClientFactory = new();
//     private readonly InterviewService _service;
//     private readonly Mock<IMongoCollection<InterviewReport>> _mockReportCollection = new();
//     private readonly Mock<IMongoCollection<InterviewSession>> _mockSessionCollection = new();

//     public InterviewServiceAdditionalTests()
//     {
//         _mockDbContext.SetupGet(x => x.Sessions).Returns(_mockSessionCollection.Object);
//         _mockDbContext.SetupGet(x => x.Reports).Returns(_mockReportCollection.Object);

//         var mockHttpClient = new HttpClient(new MockHttpMessageHandler());
//         _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(mockHttpClient);

//         _service = new InterviewService(_mockDbContext.Object, _mockApiClient.Object, _mockHttpClientFactory.Object);
//     }

//     [Fact]
//     public void GetQuestions_ReturnsQuestionTexts()
//     {
//         var session = new InterviewSession
//         {
//             Questions = new List<Question>
//             {
//                 new() { Text = "Q1" },
//                 new() { Text = "Q2" }
//             }
//         };

//         var result = _service.GetQuestions(session);

//         Assert.Equal(new[] { "Q1", "Q2" }, result);
//     }

//     [Fact]
//     public void CurrentIndex_ReturnsCorrectIndex()
//     {
//         var session = new InterviewSession { CurrentIndex = 3 };
//         var index = _service.CurrentIndex(session);
//         Assert.Equal(3, index);
//     }

//     [Fact]
//     public async Task GetReportByIdAsync_ReturnsCorrectReport()
//     {
//         var reportId = "r1";
//         var report = new InterviewReport { Id = reportId };

//         var cursor = MockCursor(new List<InterviewReport> { report });

//         _mockReportCollection.As<IAsyncCursorSource<InterviewReport>>()
//             .Setup(x => x.FindAsync(
//                 It.IsAny<FilterDefinition<InterviewReport>>(),
//                 It.IsAny<FindOptions<InterviewReport, InterviewReport>>(),
//                 It.IsAny<CancellationToken>()))
//             .ReturnsAsync(cursor.Object);

//         var result = await _service.GetReportByIdAsync(reportId);

//         Assert.NotNull(result);
//         Assert.Equal(reportId, result.Id);
//     }

//     [Fact]
//     public async Task GetReportsByEmailAsync_ReturnsReports()
//     {
//         var reports = new List<InterviewReport>
//         {
//             new() { Id = "1" }, new() { Id = "2" }
//         };

//         var cursor = MockCursor(reports);

//         _mockReportCollection.As<IAsyncCursorSource<InterviewReport>>()
//             .Setup(x => x.FindAsync(
//                 It.IsAny<FilterDefinition<InterviewReport>>(),
//                 It.IsAny<FindOptions<InterviewReport, InterviewReport>>(),
//                 It.IsAny<CancellationToken>()))
//             .ReturnsAsync(cursor.Object);

//         var result = await _service.GetReportsByEmailAsync("test@example.com");

//         Assert.Equal(2, result.Count);
//     }

//     private static Mock<IAsyncCursor<T>> MockCursor<T>(List<T> items)
//     {
//         var cursor = new Mock<IAsyncCursor<T>>();
//         cursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
//               .ReturnsAsync(true)
//               .ReturnsAsync(false);
//         cursor.Setup(c => c.Current).Returns(items);
//         return cursor;
//     }

//     private class MockHttpMessageHandler : HttpMessageHandler
//     {
//         protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
//         {
//             var json = JsonSerializer.Serialize(new
//             {
//                 score = 85,
//                 strengths = new[] { "Clarity", "Confidence" },
//                 improvements = new[] { "Pacing" },
//                 followUps = new[] { "Can you elaborate more?" },
//                 jd = "JD Sample",
//                 questions = new[] { "Q1", "Q2" }
//             });

//             return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
//             {
//                 Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
//             });
//         }
//     }
// }