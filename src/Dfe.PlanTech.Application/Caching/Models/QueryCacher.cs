using Dfe.PlanTech.Domain.Caching.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Dfe.PlanTech.Application.Caching.Models;

public class QueryCacher : IQueryCacher
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private const int CacheDurationMinutes = 30;

    public async Task<QueryCacheResult<TResult>> GetOrCreateAsyncWithCache<T, TResult>(
        string key,
        IQueryable<T> queryable,
        Func<IQueryable<T>, CancellationToken, Task<TResult>> queryFunc,
        CancellationToken cancellationToken = default)
    {
        if (TryGetFromCache<TResult>(key, out var cachedResult) && cachedResult is not null)
        {
            return cachedResult;
        }

        return await FetchAndCacheAsync(key, queryable, queryFunc, cancellationToken);
    }

    private bool TryGetFromCache<TResult>(string key, out QueryCacheResult<TResult>? queryCacheResult)
    {
        if (_cache.TryGetValue(key, out TResult? cachedEntry) && cachedEntry != null)
        {
            queryCacheResult = new(cachedEntry, CacheRetrievalSource.Cache);
            return true;
        }

        queryCacheResult = null;
        return false;
    }

    private async Task<QueryCacheResult<TResult>> FetchAndCacheAsync<T, TResult>(
        string key,
        IQueryable<T> queryable,
        Func<IQueryable<T>, CancellationToken, Task<TResult>> queryFunc,
        CancellationToken cancellationToken = default)
    {
        var retrievedEntry = await queryFunc(queryable, cancellationToken);
        _cache.Set(key, retrievedEntry, TimeSpan.FromMinutes(CacheDurationMinutes));

        return new QueryCacheResult<TResult>(retrievedEntry, CacheRetrievalSource.Db);
    }

    public void ClearCache()
    {
        _cache.Clear();
    }
}
