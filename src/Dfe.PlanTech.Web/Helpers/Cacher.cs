using System.Text.Json;
using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace Dfe.PlanTech.Web.Helpers;

/// <summary>
/// Retrieves + stores values in IMemoryCache
/// </summary>
public class Cacher : ICacher
{
    private readonly IDistributedCache _cache;
    private readonly JsonSerializerOptions _jsonOptions;

    public Cacher(IDistributedCache distributedCache)
    {
        _cache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        _jsonOptions = new JsonSerializerOptions();
    }

    public Cacher(IDistributedCache distributedCache, JsonSerializerOptions jsonOptions)
    {
        _cache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        _jsonOptions = jsonOptions;
    }


    public async Task<T?> GetAsync<T>(string key, Func<Task<T>> getFromService, TimeSpan? timeToLive = null)
    {
        var fromCache = await GetAsync<T>(key);

        if (fromCache != null)
        {
            return fromCache;
        }

        var fromService = await getFromService();

        await SetAsync(key, fromService, timeToLive);

        return fromService;
    }


    public async Task<T?> GetAsync<T>(string key, Func<T> getFromService, TimeSpan? timeToLive = null)
    {
        var fromCache = await GetAsync<T>(key);

        if (fromCache != null)
        {
            return fromCache;
        }

        var fromService = getFromService();

        await SetAsync(key, fromService, timeToLive);

        return fromService;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var cachedItem = await _cache.GetStringAsync(key);

        if (string.IsNullOrEmpty(cachedItem))
            return default;

        var deserialised = JsonSerializer.Deserialize<CachedItem<T>>(cachedItem, _jsonOptions) ??
                            throw new InvalidCastException($"Expected type of {typeof(CachedItem<T>)} but could not serialise");

        return deserialised.Item;
    }



    public async Task SetAsync<T>(string key, T? value, TimeSpan? timeToLive = null)
    {
        if (value == null)
        {
            await _cache.RemoveAsync(key);
            return;
        }

        var cachedItem = new CachedItem<T>()
        {
            Item = value
        };

        string? asString = JsonSerializer.Serialize(cachedItem, _jsonOptions);

        if (timeToLive != null)
        {
            await SetWithOptionsAsync(key, timeToLive, asString);
        }
        else
        {
            await _cache.SetStringAsync(key, asString);
        }
    }

    private async Task SetWithOptionsAsync(string key, TimeSpan? timeToLive, string asString)
    {
        var options = new DistributedCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = timeToLive
        };

        await _cache.SetStringAsync(key, asString, options);
    }
}
