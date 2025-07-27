using MongoDB.Driver;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Helper methods for mocking MongoDB's IFindFluent for unit tests.
/// </summary>
public static class MongoMockHelpers
{
    public static Mock<IFindFluent<T, T>> MockFindFluent<T>(IEnumerable<T> docs)
    {
        var findFluent = new Mock<IFindFluent<T, T>>();
        // Synchronous returns
        findFluent.Setup(x => x.ToList(It.IsAny<CancellationToken>()))
            .Returns(docs.ToList());
        findFluent.Setup(x => x.FirstOrDefault(It.IsAny<CancellationToken>()))
            .Returns(docs.FirstOrDefault());
        // Asynchronous returns
        findFluent.Setup(x => x.ToListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(docs.ToList());
        findFluent.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(docs.FirstOrDefault());
        return findFluent;
    }
}
