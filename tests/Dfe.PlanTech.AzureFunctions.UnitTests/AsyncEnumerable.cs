using System.Linq.Expressions;

namespace Dfe.PlanTech.AzureFunctions.UnitTests
{
  public class AsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
  {
    public AsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }

    public AsyncEnumerable(Expression expression) : base(expression) { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
      return new AsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => this;
  }
}