using Dfe.PlanTech.Application.Caching.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Dfe.PlanTech.Application.Caching.Models;

public class QueryCacher: IQueryCacher
{
    private MemoryCache _cache = new(new MemoryCacheOptions());
    private const int CacheDurationMinutes = 30;

    public async Task<TResult> GetOrCreateAsyncWithCache<T, TResult>(
        string key,
        IQueryable<T> queryable,
        Func<IQueryable<T>, CancellationToken, Task<TResult>> queryFunc,
        CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheDurationMinutes);
            return queryFunc(queryable, cancellationToken);
        }) ?? await queryFunc(queryable, cancellationToken);
    }

    public void ClearCache()
    {
        _cache.Dispose();
        _cache = new MemoryCache(new MemoryCacheOptions());
    }
}
