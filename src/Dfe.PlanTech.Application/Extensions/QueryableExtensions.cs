using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Dfe.PlanTech.Application.Extensions;

/// <summary>
/// Extension methods for caching database commands by QueryString
/// These do not have any cache invalidation and should only be used on CMSDbContext queries, not PlanTechDbContext
/// </summary>
public static class QueryableExtensions
{
    private static MemoryCache _cache = new(new MemoryCacheOptions());
    private const int CacheDurationMinutes = 30;

    private static string GetCacheKey(IQueryable query)
    {
        var queryString = query.ToQueryString();
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(queryString));
        return Convert.ToBase64String(hash);
    }

    private static async Task<TResult> GetOrCreateAsyncWithCache<T, TResult>(
        IQueryable<T> queryable,
        Func<IQueryable<T>, CancellationToken, Task<TResult>> queryFunc,
        CancellationToken cancellationToken = default)
    {
        var key = GetCacheKey(queryable);
        return await _cache.GetOrCreateAsync(key, cacheEntry =>
        {
            cacheEntry.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(CacheDurationMinutes);
            return queryFunc(queryable, cancellationToken);
        }) ?? await queryFunc(queryable, cancellationToken);
    }

    public static Task<List<T>> ToListAsyncWithCache<T>(this IQueryable<T> queryable, CancellationToken cancellationToken = default)
    {
        return GetOrCreateAsyncWithCache(queryable, (q, ctoken) => q.ToListAsync(ctoken), cancellationToken);
    }

    public static Task<T?> FirstOrDefaultAsyncWithCache<T>(this IQueryable<T> queryable, CancellationToken cancellationToken = default)
    {
        return GetOrCreateAsyncWithCache(queryable, (q, ctoken) => q.FirstOrDefaultAsync(ctoken), cancellationToken);
    }

    public static void ClearCmsCache()
    {
        _cache.Dispose();
        _cache = new MemoryCache(new MemoryCacheOptions());
    }
}
