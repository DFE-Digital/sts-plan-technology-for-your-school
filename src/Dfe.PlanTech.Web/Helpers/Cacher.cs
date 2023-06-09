using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Domain.Caching.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Dfe.PlanTech.Web.Helpers;

/// <summary>
/// Retrieves + stores values in IMemoryCache
/// </summary>
public class Cacher : ICacher
{
    private readonly ICacheOptions _options;

    private readonly IMemoryCache _memoryCache;

    public Cacher(ICacheOptions options, IMemoryCache memoryCache)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public T? Get<T>(string key, Func<T> getFromService, TimeSpan timeToLive)
    {
        if (_memoryCache.TryGetValue(key, out T? value))
        {
            return value;
        }

        value = getFromService();

        Set(key, timeToLive, value);

        return value;
    }

    public T? Get<T>(string key)
    {
        _memoryCache.TryGetValue(key, out T? value);

        return value;
    }

    public T? Get<T>(string key, Func<T> getFromService) => Get(key, getFromService, _options.DefaultTimeToLive);

    public async Task<T?> GetAsync<T>(string key, Func<Task<T>> getFromService, TimeSpan timeToLive)
    {
        if (_memoryCache.TryGetValue(key, out T? value))
        {
            return value;
        }

        value = await getFromService();

        Set(key, timeToLive, value);

        return value;
    }

    public Task<T?> GetAsync<T>(string key, Func<Task<T>> getFromService) => GetAsync(key, getFromService, _options.DefaultTimeToLive);

    public void Set<T>(string key, TimeSpan timeToLive, T? value)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(timeToLive);

        _memoryCache.Set(key, value, cacheEntryOptions);
    }
}
