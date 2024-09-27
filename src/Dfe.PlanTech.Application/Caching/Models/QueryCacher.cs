using System.Security.Cryptography;
using System.Text;
using Dfe.PlanTech.Application.Caching.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Dfe.PlanTech.Application.Caching.Models;

public class QueryCacher: IQueryCacher
{
    private MemoryCache _cache = new(new MemoryCacheOptions());
    private const int CacheDurationMinutes = 30;

    private static string GetCacheKey(IQueryable query)
    {
        var queryString = query.ToQueryString();
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(queryString));
        return Convert.ToBase64String(hash);
    }

    public async Task<TResult> GetOrCreateAsyncWithCache<T, TResult>(
        IQueryable<T> queryable,
        Func<IQueryable<T>, CancellationToken, Task<TResult>> queryFunc,
        CancellationToken cancellationToken = default)
    {
        var key = GetCacheKey(queryable);
        return await _cache.GetOrCreateAsync(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheDurationMinutes);
            return queryFunc(queryable, cancellationToken);
        }) ?? await queryFunc(queryable, cancellationToken);
    }

    public async Task<List<T>> ToListAsyncWithCache<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default)
    {
        return await GetOrCreateAsyncWithCache(queryable, (q, ctoken) => q.ToListAsync(ctoken), cancellationToken);
    }

    public async Task<T?> FirstOrDefaultAsyncWithCache<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default)
    {
        return await GetOrCreateAsyncWithCache(queryable, (q, ctoken) => q.FirstOrDefaultAsync(ctoken), cancellationToken);
    }

    public void ClearCmsCache()
    {
        _cache.Dispose();
        _cache = new MemoryCache(new MemoryCacheOptions());
    }
}
