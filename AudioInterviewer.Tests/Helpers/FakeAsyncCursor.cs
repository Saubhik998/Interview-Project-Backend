using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AudioInterviewer.Tests.Helpers
{
    public class FakeAsyncCursor<T> : IAsyncCursor<T>
    {
        private readonly IEnumerator<List<T>> _enumerator;
        public IEnumerable<T> Current => _enumerator.Current;

        public FakeAsyncCursor(IEnumerable<T> items)
        {
            _enumerator = new List<List<T>> { items.ToList() }.GetEnumerator();
        }
        public void Dispose() => _enumerator.Dispose();
        public bool MoveNext(CancellationToken cancellationToken = default) => _enumerator.MoveNext();
        public bool MoveNext() => _enumerator.MoveNext();
        public Task<bool> MoveNextAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_enumerator.MoveNext());
    }
}
