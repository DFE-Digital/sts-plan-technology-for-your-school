using Dfe.PlanTech.Domain.Caching.Models;

namespace Dfe.PlanTech.Domain.Caching.Interfaces;

public interface IQueryCacher
{
    public Task<QueryCacheResult<TResult>> GetOrCreateAsyncWithCache<T, TResult>(
        string key,
        IQueryable<T> queryable,
        Func<IQueryable<T>, CancellationToken, Task<TResult>> queryFunc,
        CancellationToken cancellationToken = default);

    public void ClearCache();
}
