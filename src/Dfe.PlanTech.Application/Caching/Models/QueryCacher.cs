using Dfe.PlanTech.Domain.Caching.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Caching.Models;

public class QueryCacher(ILogger<QueryCacher> logger) : IQueryCacher
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
        logger.LogTrace("Attempting to retrieve key \"{Key}\" from the cache as type \"{Type}\"", key, typeof(TResult));
        if (_cache.TryGetValue(key, out TResult? cachedEntry) && EqualityComparer<TResult>.Default.Equals(cachedEntry, default))
        {
            queryCacheResult = new(cachedEntry, CacheRetrievalSource.Cache);
            logger.LogTrace("Retrieved key \"{Key}\" from the cache", key);
            return true;
        }

        queryCacheResult = null;
        logger.LogTrace("Key \"{Key}\" not found in cache", key);
        return false;
    }

    private async Task<QueryCacheResult<TResult>> FetchAndCacheAsync<T, TResult>(
        string key,
        IQueryable<T> queryable,
        Func<IQueryable<T>, CancellationToken, Task<TResult>> queryFunc,
        CancellationToken cancellationToken = default)
    {
        logger.LogTrace("Retrieving value for \"{Key}\" from DB", key);

        var retrievedEntry = await queryFunc(queryable, cancellationToken);

        _cache.Set(key, retrievedEntry, TimeSpan.FromMinutes(CacheDurationMinutes));
        logger.LogTrace("Saved value for \"{Key}\" in cache", key);
        return new QueryCacheResult<TResult>(retrievedEntry, CacheRetrievalSource.Db);
    }

    public void ClearCache()
    {
        _cache.Clear();
    }
}
