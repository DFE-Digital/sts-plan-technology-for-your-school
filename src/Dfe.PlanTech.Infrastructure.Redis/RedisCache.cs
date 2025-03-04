using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Domain.Background;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using StackExchange.Redis;

namespace Dfe.PlanTech.Infrastructure.Redis;

/// <summary>
/// Represents a Redis-based implementation of a distributed cache.
/// </summary>
public class RedisCache : ICmsCache
{
    private readonly IRedisConnectionManager _connectionManager;
    private readonly AsyncRetryPolicy _retryPolicyAsync;
    private readonly ILogger<RedisCache> _logger;
    private readonly IRedisDependencyManager _dependencyManager;
    private readonly IBackgroundTaskQueue _backgroundTaskService;

    public RedisCache(IRedisConnectionManager connectionManager, ILogger<RedisCache> logger, IRedisDependencyManager dependencyManager, IBackgroundTaskQueue backgroundTaskQueue)
    {
        _connectionManager = connectionManager;
        _logger = logger;

        var retryPolicyBuilder = Policy.Handle<TimeoutException>()
            .Or<RedisServerException>()
            .Or<RedisException>()
            .OrInner<TimeoutException>()
            .OrInner<RedisServerException>()
            .OrInner<RedisException>();

        _retryPolicyAsync = retryPolicyBuilder.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(50));
        _dependencyManager = dependencyManager;
        _backgroundTaskService = backgroundTaskQueue;
    }

    /// <inheritdoc/>
    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> action, TimeSpan? expiry = null, Func<T, Task>? onCacheItemCreation = null, int databaseId = -1)
    {
        _logger.LogInformation("Attempting to get or create cache item with key: {Key}", key);

        try
        {
            var db = await _connectionManager.GetDatabaseAsync(databaseId);
            var redisResult = await GetAsync<T>(db, key);

            var existedInCache = redisResult.ExistedInCache == true && redisResult.CacheValue != null;
            var hasContent = false;

            if (existedInCache)
            {
                var resultType = redisResult.CacheValue!.GetType();

                if (typeof(IEnumerable<IContentComponent>).IsAssignableFrom(resultType))
                {
                    var cacheValue = redisResult.CacheValue as IEnumerable<IContentComponent>;
                    hasContent = cacheValue != null && cacheValue.Any();
                }
                else
                {
                    var cacheValue = redisResult.CacheValue as IContentComponent;
                    hasContent = cacheValue != null;
                }
            }

            if (existedInCache && hasContent)
            {
                _logger.LogTrace("Cache item with key: {Key} found", key);
                return redisResult.CacheValue;
            }
            else if (redisResult.Errored)
            {
                return await action();
            }

            return await CreateAndCacheItemAsync(db, key, action, expiry, onCacheItemCreation);
        }
        catch (RedisConnectionException redisException)
        {
            _logger.LogError(redisException, "Failed to connect to Redis server: \"{Message}\". Retrieving server using {Action}.", redisException.Message, nameof(Action));
            return await action();
        }
    }

    /// <inheritdoc/>
    public async Task<string> SetAsync<T>(string key, T value, TimeSpan? expiry = null, int databaseId = -1)
    {
        _logger.LogInformation("Setting cache item with key: {Key}", key);
        var database = await _connectionManager.GetDatabaseAsync(databaseId);
        await _dependencyManager.RegisterDependenciesAsync(database, key, value);
        return await SetAsync(database, key, value, expiry);
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveAsync(string key, int databaseId = -1)
    {
        _logger.LogInformation("Removing cache item with key: {Key}", key);
        var database = await _connectionManager.GetDatabaseAsync(databaseId);
        return await RemoveAsync(database, key);
    }

    /// <inheritdoc/>
    public Task RemoveAsync(params string[] keys)
    {
        _logger.LogInformation("Removing cache items with keys: {Keys}", string.Join(", ", keys));
        return Task.WhenAll(keys.Select(key => RemoveAsync(key)));
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(int databaseId, params string[] keys)
    {
        _logger.LogInformation("Removing cache items with keys: {Keys} from database {DatabaseId}", string.Join(", ", keys), databaseId);
        var database = await _connectionManager.GetDatabaseAsync(databaseId);
        await Task.WhenAll(keys.Select(key => RemoveAsync(database, key)));
    }

    /// <inheritdoc/>
    public async Task AppendAsync(string key, string item, int databaseId = -1)
    {
        _logger.LogInformation("Appending item to cache with key: {Key}", key);
        await (await _connectionManager.GetDatabaseAsync(databaseId)).StringAppendAsync(key, item);
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string key, int databaseId = -1)
    {
        _logger.LogInformation("Retrieving cache item with key: {Key}", key);
        var database = await _connectionManager.GetDatabaseAsync(databaseId);
        var result = await GetAsync<T>(database, key);
        return result.CacheValue;
    }

    /// <inheritdoc/>
    public async Task SetAddAsync(string key, string item, int databaseId = -1)
    {
        _logger.LogInformation("Adding item to set with key: {Key}", key);

        ArgumentNullException.ThrowIfNull(item);

        var database = await _connectionManager.GetDatabaseAsync(databaseId);
        await _retryPolicyAsync.ExecuteAsync(() => database.SetAddAsync(key, item));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<string>> GetSetMembersAsync(string key, int databaseId = -1)
    {
        _logger.LogInformation("Getting set members for key: {Key}", key);
        return await _retryPolicyAsync.ExecuteAsync(async () => (await (await _connectionManager.GetDatabaseAsync(databaseId)).SetMembersAsync(key)).Select(x => x.ToString()));
    }

    /// <inheritdoc/>
    public Task SetRemoveAsync(string key, string item, int databaseId = -1)
    {
        _logger.LogInformation("Removing item from set with key: {Key}", key);
        return _retryPolicyAsync.ExecuteAsync(async () => (await _connectionManager.GetDatabaseAsync(databaseId)).SetRemoveAsync(key, item));
    }

    /// <inheritdoc/>
    public async Task SetRemoveItemsAsync(string key, IEnumerable<string> items, int databaseId = -1)
    {
        _logger.LogInformation("Removing multiple items from set with key: {Key}", key);
        var database = await _connectionManager.GetDatabaseAsync(databaseId);
        await _retryPolicyAsync.ExecuteAsync(() => database.SetRemoveAsync(key, [.. items.Select(x => (RedisValue)x)]));
    }

    /// <inheritdoc/>
    private async Task<string> SetAsync<T>(IDatabase database, string key, T value, TimeSpan? expiry = null)
    {
        var redisValue = value as string ?? value.Serialise();
        _logger.LogInformation("Setting cache item with key: {Key} and value: {Value}", key, redisValue);
        await _retryPolicyAsync.ExecuteAsync(() => database.StringSetAsync(key, GZipRedisValueCompressor.Compress(redisValue), expiry));
        await _dependencyManager.RegisterDependenciesAsync(database, key, value, default);
        return key;
    }

    /// <summary>
    /// Gets an item from the provided Redis.
    /// </summary>
    /// <typeparam name="T">Type of expected result; will be serialised from JSON</typeparam>
    /// <param name="database">Redis database to fetch from</param>
    /// <param name="key">Key of the value to fetch</param>
    /// <returns></returns>
    private async Task<CacheResult<T>> GetAsync<T>(IDatabase database, string key)
    {
        try
        {
            if (string.IsNullOrEmpty(key))
            {
                _logger.LogError("Attempt made to retrieve items with empty key");
                return new CacheResult<T>(Error: "Key is null or empty");
            }

            _logger.LogInformation("Getting cache item with key: {Key}", key);
            var redisResult = await _retryPolicyAsync.ExecuteAsync(async () => GZipRedisValueCompressor.Decompress(await database.StringGetAsync(key)));

            return CreateCacheResult<T>(redisResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching {Key} from cache", key);
            return new CacheResult<T>(Error: ex.Message);
        }
    }

    /// <summary>
    /// Removes an item from the Redis database
    /// </summary>
    /// <param name="database">Database to remove the item from</param>
    /// <param name="key">Key of the item to remove</param>
    /// <returns>True if successful, otherwise false</returns>
    private static Task<bool> RemoveAsync(IDatabase database, string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return Task.FromResult(false);
        }

        return database.KeyDeleteAsync(key);
    }

    /// <summary>
    /// Creates a <see cref="CacheResult{T}"/> from the provided Redis result.
    /// </summary>
    /// <typeparam name="T">Type of the expected result</typeparam>
    /// <param name="redisResult">Result from the Redis get operation</param>
    /// <returns></returns>
    private static CacheResult<T> CreateCacheResult<T>(RedisValue redisResult)
    {
        if (!redisResult.HasValue)
        {
            return new CacheResult<T>(ExistedInCache: false);
        }

        return redisResult is T typed
            ? new CacheResult<T>(ExistedInCache: true, CacheValue: typed)
            : new CacheResult<T>(ExistedInCache: true, CacheValue: redisResult.Deserialise<T>());
    }

    /// <summary>
    /// Creates a new cache item and stores it in the cache.
    /// </summary>
    /// <typeparam name="T">The type of the cached item.</typeparam>
    /// <param name="db">The database connection to use for caching.</param>
    /// <param name="key">The key associated with the cached item.</param>
    /// <param name="action">A function that creates the item asynchronously.</param>
    /// <param name="expiry">Optional. The time span after which the cached item expires.</param>
    /// <param name="onCacheItemCreation">Optional. A function that is called after the item is created and cached.</param>
    /// <returns>The newly created cached item or default value if the action returned null.</returns>
    private async Task<T?> CreateAndCacheItemAsync<T>(IDatabase db, string key, Func<Task<T>> action, TimeSpan? expiry, Func<T, Task>? onCacheItemCreation)
    {
        _logger.LogTrace("Cache item with key: {Key} not found, executing action to create it", key);
        var result = await action();

        if (EqualityComparer<T>.Default.Equals(result, default))
        {
            _logger.LogWarning("Action returned null for cache item with key: {Key}", key);
            return result;
        }

        var setValue = await SetAsync(db, key, result, expiry);
        if (!string.IsNullOrEmpty(setValue))
        {
            _logger.LogInformation("Cache item with key: {Key} created and stored", key);

            if (onCacheItemCreation != null)
            {
                await onCacheItemCreation(result).ConfigureAwait(false);
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public Task InvalidateCacheAsync(string contentComponentId, string contentType)
    => _backgroundTaskService.QueueBackgroundWorkItemAsync(async (cancellationToken) =>
        {
            var key = _dependencyManager.GetDependencyKey(contentComponentId);
            await RemoveDependenciesAsync(key);

            // Invalidate all empty collections
            await RemoveDependenciesAsync(_dependencyManager.EmptyCollectionDependencyKey);

            // Invalidate collection of the content type if there is one
            await RemoveAsync($"{contentType}s");
        });

    private async Task RemoveDependenciesAsync(string dependencyKey)
    {
        var dependencies = (await GetSetMembersAsync(dependencyKey)).ToList();
        foreach (var item in dependencies)
        {
            await RemoveAsync(item);
        }
        await SetRemoveItemsAsync(dependencyKey, dependencies);
    }
}
