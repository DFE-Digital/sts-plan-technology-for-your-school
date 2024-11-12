using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Web.Models.Content.Mapped;
using Microsoft.Extensions.Caching.Memory;

namespace Dfe.PlanTech.Web.Content;

[ExcludeFromCodeCoverage]
public class CsPagesCacheService(IMemoryCache cache, IConfiguration configuration)
    : ICacheService<List<CsPage>>
{
    private readonly int _cacheTimeOutMs = configuration.GetSection("CacheTimeOutMs").Get<int>();

    public void AddToCache(string key, List<CsPage> item)
    {
        MemoryCacheEntryOptions options = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(_cacheTimeOutMs)
        };
        cache.Set(key, item, options);
    }

    public List<CsPage>? GetFromCache(string key)
    {
        return cache.Get<List<CsPage>>(key);
    }

    public void ClearCache()
    {
        (cache as MemoryCache)?.Clear();
    }
}
