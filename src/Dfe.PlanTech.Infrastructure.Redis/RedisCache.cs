using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using StackExchange.Redis;

namespace Dfe.PlanTech.Infrastructure.Redis;

public class RedisCache : IDistributedCache
{
    private readonly IRedisConnectionManager _connectionManager;
    private readonly AsyncRetryPolicy _retryPolicyAsync;
    private readonly ILogger<RedisCache> _logger;

    public RedisCache(IRedisConnectionManager connectionManager, ILogger<RedisCache> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;

        var retryPolicyBuilder = Policy.Handle<TimeoutException>()
            .Or<RedisServerException>()
            .Or<RedisException>()
            .OrInner<TimeoutException>()
            .OrInner<RedisServerException>()
            .OrInner<RedisException>();

        _retryPolicyAsync = retryPolicyBuilder.WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(2));
    }

    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> action, TimeSpan? expiry = null, Func<T, Task>? onCacheItemCreation = null, int databaseId = -1)
    {
        _logger.LogInformation("Attempting to get or create cache item with key: {Key}", key);

        var db = await _connectionManager.GetDatabaseAsync(databaseId);
        var redisResult = await GetAsync<T>(db, key);

        if (redisResult.ExistedInCache == true)
        {
            _logger.LogInformation("Cache item with key: {Key} found", key);
            return redisResult.CacheValue;
        }

        _logger.LogInformation("Cache item with key: {Key} not found, executing action to create it", key);
        var result = await action();
        if (result == null)
        {
            _logger.LogWarning("Action returned null for cache item with key: {Key}", key);
            return result;
        }

        await SetAsync(db, key, result, expiry);
        _logger.LogInformation("Cache item with key: {Key} created and stored", key);
        if (onCacheItemCreation != null)
        {
            await onCacheItemCreation(result).ConfigureAwait(false);
        }

        return result;
    }

    public async Task<string> SetAsync<T>(string key, T value, TimeSpan? expiry = null, int databaseId = -1)
    {
        _logger.LogInformation("Setting cache item with key: {Key}", key);
        var database = await _connectionManager.GetDatabaseAsync(databaseId);
        return await SetAsync(database, key, value, expiry);
    }

    public async Task<bool> RemoveAsync(string key, int databaseId = -1)
    {
        _logger.LogInformation("Removing cache item with key: {Key}", key);
        var database = await _connectionManager.GetDatabaseAsync(databaseId);
        return await RemoveAsync(database, key);
    }

    public Task RemoveAsync(params string[] keys)
    {
        _logger.LogInformation("Removing cache items with keys: {Keys}", string.Join(", ", keys));
        return Task.WhenAll(keys.Select(key => RemoveAsync(key)));
    }

    public async Task RemoveAsync(int databaseId, params string[] keys)
    {
        _logger.LogInformation("Removing cache items with keys: {Keys} from database {DatabaseId}", string.Join(", ", keys), databaseId);
        var database = await _connectionManager.GetDatabaseAsync(databaseId);
        await Task.WhenAll(keys.Select(key => RemoveAsync(database, key)));
    }

    public async Task AppendAsync(string key, string item, int databaseId = -1)
    {
        _logger.LogInformation("Appending item to cache with key: {Key}", key);
        await (await _connectionManager.GetDatabaseAsync(databaseId)).StringAppendAsync(key, item);
    }

    public async Task<T?> GetAsync<T>(string key, int databaseId = -1)
    {
        if (string.IsNullOrEmpty(key))
        {
            _logger.LogError("Attempt made to retrieve items with empty key");
            return default;
        }

        _logger.LogInformation("Retrieving cache item with key: {Key}", key);
        var database = await _connectionManager.GetDatabaseAsync(databaseId);
        var result = await GetAsync<T>(database, key);
        return result.CacheValue;
    }

    public async Task SetAddAsync(string key, string item, int databaseId = -1)
    {
        _logger.LogInformation("Adding item to set with key: {Key}", key);
        var database = await _connectionManager.GetDatabaseAsync(databaseId);
        await _retryPolicyAsync.ExecuteAsync(() => database.SetAddAsync(key, item ?? throw new ArgumentNullException(nameof(item))));
    }

    public async Task<string[]> GetSetMembersAsync(string key, int databaseId = -1)
    {
        _logger.LogInformation("Getting set members for key: {Key}", key);
        return await _retryPolicyAsync.ExecuteAsync(async () => (await (await _connectionManager.GetDatabaseAsync(databaseId)).SetMembersAsync(key)).Select(x => x.ToString()).ToArray() ?? []);
    }

    public Task SetRemoveAsync(string key, string item, int databaseId = -1)
    {
        _logger.LogInformation("Removing item from set with key: {Key}", key);
        return _retryPolicyAsync.ExecuteAsync(async () => (await _connectionManager.GetDatabaseAsync(databaseId)).SetRemoveAsync(key, item));
    }

    public async Task SetRemoveItemsAsync(string key, string[] items, int databaseId = -1)
    {
        _logger.LogInformation("Removing multiple items from set with key: {Key}", key);
        var database = await _connectionManager.GetDatabaseAsync(databaseId);
        await _retryPolicyAsync.ExecuteAsync(() => database.SetRemoveAsync(key, items.Select(x => (RedisValue)x).ToArray()));
    }

    private async Task<string> SetAsync<T>(IDatabase database, string key, T value, TimeSpan? expiry = null)
    {
        var redisValue = value as string ?? value.Serialise();
        _logger.LogInformation("Setting cache item with key: {Key} and value: {Value}", key, redisValue);
        await _retryPolicyAsync.ExecuteAsync(() => database.StringSetAsync(key, GZipRedisValueCompressor.Compress(redisValue), expiry));
        return key;
    }

    private async Task<CacheResult<T>> GetAsync<T>(IDatabase database, string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            _logger.LogError("Attempt made to retrieve items with empty key");
            return new CacheResult<T>(Error: "Key is null or empty");
        }

        _logger.LogInformation("Getting cache item with key: {Key}", key);
        var redisResult = await _retryPolicyAsync.ExecuteAsync(async () => GZipRedisValueCompressor.Decompress(await database.StringGetAsync(key)));

        return !redisResult.HasValue
            ? new CacheResult<T>(ExistedInCache: false)
            : redisResult is T typed
                ? new CacheResult<T>(ExistedInCache: true, CacheValue: typed)
                : new CacheResult<T>(ExistedInCache: true, CacheValue: redisResult.Deserialise<T>());
    }

    private static Task<bool> RemoveAsync(IDatabase database, string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return Task.FromResult(false);
        }

        return database.KeyDeleteAsync(key);
    }
}
