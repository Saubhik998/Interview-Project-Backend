using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AudioInterviewer.Tests.Helpers
{
    public class FakeFindFluent<T> : IFindFluent<T, T>, IEnumerable<T>, IEnumerable
    {
        private readonly List<T> _docs;

        public FakeFindFluent(IEnumerable<T> docs)
        {
            _docs = docs?.ToList() ?? new List<T>();
        }

        public List<T> ToList(CancellationToken cancellationToken = default) => _docs;
        public Task<List<T>> ToListAsync(CancellationToken cancellationToken = default) => Task.FromResult(_docs);
        public T? FirstOrDefault(CancellationToken cancellationToken = default) => _docs.FirstOrDefault();
        public Task<T?> FirstOrDefaultAsync(CancellationToken cancellationToken = default) => Task.FromResult(_docs.FirstOrDefault());

        public IEnumerator<T> GetEnumerator() => _docs.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _docs.GetEnumerator();

        public IFindFluent<T, T> Limit(int? limit) => this;
        public IFindFluent<T, T> Skip(int? skip) => this;
        public IFindFluent<T, T> Sort(SortDefinition<T> sort) => this;
        public IFindFluent<T, T> BatchSize(int? batchSize) => this;
        public IFindFluent<T, T> MaxTime(TimeSpan? maxTime) => this;

        public long? MaxTimeMS => null;

        public IAsyncCursor<T> ToCursor(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IAsyncCursor<T>> ToCursorAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public IFindFluent<T, TNewProjection> Project<TNewProjection>(ProjectionDefinition<T, TNewProjection> projection) => throw new NotImplementedException();
        public IFindFluent<T, TNewResult> As<TNewResult>(IBsonSerializer<TNewResult> serializer) => throw new NotImplementedException();

        public FilterDefinition<T> Filter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public FindOptions<T, T> Options => throw new NotImplementedException();
        public IBsonSerializer<T> Serializer => throw new NotImplementedException();
        public IMongoCollection<T> Collection => throw new NotImplementedException();

        public long Count(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<long> CountAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public long CountDocuments(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<long> CountDocumentsAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public string ToString(ExpressionTranslationOptions options) => throw new NotImplementedException();
    }
}
