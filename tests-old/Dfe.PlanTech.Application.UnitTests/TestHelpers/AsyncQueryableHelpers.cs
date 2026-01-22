using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Dfe.PlanTech.Web.UnitTests
{
    public static class AsyncQueryableHelpers
    {
        public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable)
                : base(enumerable) { }

            public TestAsyncEnumerable(Expression expression)
                : base(expression) { }

            public IAsyncEnumerator<T> GetAsyncEnumerator(
                CancellationToken cancellationToken = default
            ) => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
        }

        public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;

            public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;

            public T Current => _inner.Current;

            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return ValueTask.CompletedTask;
            }

            public ValueTask<bool> MoveNextAsync() => new(_inner.MoveNext());
        }

        public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;

            public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

            public IQueryable CreateQuery(Expression expression) =>
                new TestAsyncEnumerable<TEntity>(expression);

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
                new TestAsyncEnumerable<TElement>(expression);

            public object Execute(Expression expression) => _inner.Execute(expression)!;

            public TResult Execute<TResult>(Expression expression) =>
                _inner.Execute<TResult>(expression)!;

            public TResult ExecuteAsync<TResult>(
                Expression expression,
                CancellationToken cancellationToken = default
            ) => Task.FromResult(Execute<TResult>(expression)).Result;
        }
    }
}
