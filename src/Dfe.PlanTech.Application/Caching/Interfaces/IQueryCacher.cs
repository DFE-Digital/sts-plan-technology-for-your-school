namespace Dfe.PlanTech.Application.Caching.Interfaces;

public interface IQueryCacher
{
    public Task<TResult> GetOrCreateAsyncWithCache<T, TResult>(
        IQueryable<T> queryable,
        Func<IQueryable<T>, CancellationToken, Task<TResult>> queryFunc,
        CancellationToken cancellationToken = default);

    public void ClearCmsCache();
}
