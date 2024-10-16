using Polly;
using Polly.Retry;
using StackExchange.Redis;
using System.Text.Json;
using Dfe.PlanTech.Domain.Helpers;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Caching.Interfaces;

namespace Dfe.PlanTech.Infrastructure.Redis;

public class RedisCache : IDistCache
{
    private ConnectionMultiplexer? _connectionMultiplexer = null;
    private readonly RetryPolicy _retryPolicy;
    private readonly AsyncRetryPolicy _retryPolicyAsync;
    private readonly RedisConnectionString _redisConnectionString;

    private ConnectionMultiplexer Connection => _connectionMultiplexer ?? throw new InvalidOperationException($"The {nameof(RedisCache)} object has not be initialised.");

    private const int DistLockAcquisitionTimeoutInSeconds = 30;
    private const int DistLockMaxDurationInSeconds = 200;
    public RedisCache(RedisConnectionString redisConnectionString)
    {
        _redisConnectionString = redisConnectionString;

        var pred = Policy.Handle<TimeoutException>().Or<RedisServerException>().Or<RedisException>()
            .OrInner<TimeoutException>().OrInner<RedisServerException>().OrInner<RedisException>();

        _retryPolicy = pred.WaitAndRetry(3, x => TimeSpan.FromSeconds(2));
        _retryPolicyAsync = pred.WaitAndRetryAsync(3, x => TimeSpan.FromSeconds(2));
    }

    public async Task InitialiseAsync()
    {
        _connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(_redisConnectionString);
    }

    public T? GetOrCreate<T>(string key, Func<T> action, TimeSpan? expiry = null, Action<T>? onCacheItemCreation = null, int databaseId = -1)
    {
        var db = Connection.GetDatabase(databaseId);
        var redisResult = _retryPolicy.Execute(() => db.StringGet(key));

        if (redisResult.HasValue)
        {
            return JsonSerializer.Deserialize<T>(GZipRedisValueCompressor.Decompress(redisResult));
        }

        var result = action();

        if (result != null)
        {
            _retryPolicy.Execute(() => db.StringSet(key, GZipRedisValueCompressor.Compress(JsonSerializer.Serialize(result)), expiry));
            onCacheItemCreation?.Invoke(result);
        }

        return result;
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> action, TimeSpan? expiry = null, Func<T, Task> onCacheItemCreation = null, int databaseId = -1)
    {
        var db = Connection.GetDatabase(databaseId);
        var redisResult = await _retryPolicyAsync.ExecuteAsync(async () => GZipRedisValueCompressor.Decompress(await db.StringGetAsync(key).ConfigureAwait(false))).ConfigureAwait(false);
        if (redisResult.HasValue)
        {
            return JsonSerializer.Deserialize<T>(redisResult);
        }

        var result = await action();

        if (result != null)
        {
            await _retryPolicyAsync.ExecuteAsync(async () => await db.StringSetAsync(key, GZipRedisValueCompressor.Compress(JsonSerializer.Serialize(result)), expiry).ConfigureAwait(false)).ConfigureAwait(false);
            if (onCacheItemCreation != null)
            {
                await onCacheItemCreation(result).ConfigureAwait(false);
            }
        }
        return result;
    }

    public async Task<string> SetAsync<T>(string key, T value, TimeSpan? expiry = null, int databaseId = -1)
    {
        var db = Connection.GetDatabase(databaseId);
        RedisValue v = typeof(T) == typeof(string) ? value as string : JsonSerializer.Serialize(value);
        await _retryPolicyAsync.ExecuteAsync(async () => await db.StringSetAsync(key, GZipRedisValueCompressor.Compress(v), expiry));
        return key;
    }

    public async Task<bool> RemoveAsync(string key, int databaseId = -1)
    {
        if (key != null)
        {
            return await Connection.GetDatabase(databaseId).KeyDeleteAsync(key);
        }
        else
        {
            return false;
        }
    }

    public async Task RemoveAsync(params string[] keys)
    {
        foreach (var key in keys)
        {
            await RemoveAsync(key);
        }
    }

    public async Task RemoveAsync(int databaseId, params string[] keys)
    {
        foreach (var key in keys)
        {
            await RemoveAsync(key, databaseId);
        }
    }

    public void Remove(string key, int databaseId = -1) => Connection.GetDatabase(databaseId).KeyDelete(key);

    public void Remove(string[] keys, int databaseId = -1) => Connection.GetDatabase(databaseId).KeyDelete(keys.Select(x => (RedisKey)x).ToArray());

    public void Append(string key, string item, int databaseId = -1) => Connection.GetDatabase(databaseId).StringAppend(key, item);

    public T? Get<T>(string key, int databaseId = -1)
    {
        var redisResult = _retryPolicy.Execute(() => Connection.GetDatabase(databaseId).StringGet(key));
        return !redisResult.HasValue
            ? default
            : typeof(T) == typeof(string)
            ? (T)Convert.ChangeType(redisResult, typeof(T))
            : JsonSerializer.Deserialize<T>(redisResult);
    }

    public async Task<T?> GetAsync<T>(string key, int databaseId = -1)
    {
        if (key != null)
        {
            var redisResult = await _retryPolicy.Execute(async () => GZipRedisValueCompressor.Decompress(await Connection.GetDatabase(databaseId).StringGetAsync(key)));
            return !redisResult.HasValue
                ? default
                : typeof(T) == typeof(string)
                ? (T)Convert.ChangeType(redisResult, typeof(T))
                : JsonSerializer.Deserialize<T>(redisResult);
        }
        else
        {
            return default;
        }
    }

    public void SetAdd(string key, string item, int databaseId = -1) => _retryPolicy.Execute(() =>
        Connection.GetDatabase(databaseId).SetAdd(key, item ?? throw new Exception("item cannot be null!")));

    public async Task SetAddAsync(string key, string item, int databaseId = -1) => await _retryPolicyAsync.ExecuteAsync(async () =>
        await Connection.GetDatabase(databaseId).SetAddAsync(key, item ?? throw new Exception("item cannot be null!")));

    public string[] GetSetMembers(string key, int databaseId = -1) => _retryPolicy.Execute(() =>
        Connection.GetDatabase(databaseId).SetMembers(key)?.Select(x => x.ToString()).ToArray() ?? Array.Empty<string>());

    public async Task<string[]> GetSetMembersAsync(string key, int databaseId = -1) => await _retryPolicyAsync.ExecuteAsync(async () =>
        (await Connection.GetDatabase(databaseId).SetMembersAsync(key))?.Select(x => x.ToString()).ToArray() ?? Array.Empty<string>());

    public void SetRemove(string key, string item, int databaseId = -1) => _retryPolicy.Execute(() =>
        Connection.GetDatabase(databaseId).SetRemove(key, item ?? throw new Exception("item cannot be null!")));

    public void SetRemove(string key, string[] items, int databaseId = -1) => _retryPolicy.Execute(() =>
        Connection.GetDatabase(databaseId).SetRemove(key, items.Select(x => (RedisValue)x).ToArray()));

    public bool LockTake(string name, string lockValue, TimeSpan duration, int databaseId = -1)
        => Connection.GetDatabase(databaseId).LockTake(name, lockValue, duration, CommandFlags.DemandMaster);

    public bool LockRelease(string name, string lockValue, int databaseId = -1)
    => Connection.GetDatabase(databaseId).LockRelease(name, lockValue, CommandFlags.DemandMaster);


    public bool LockExtend(string name, string lockValue, TimeSpan duration, int databaseId = -1)
        => Connection.GetDatabase(databaseId).LockExtend(name, lockValue, duration, CommandFlags.DemandMaster);

    public async Task<bool> LockTakeAsync(string name, string lockValue, TimeSpan duration, int databaseId = -1)
        => await Connection.GetDatabase(databaseId).LockTakeAsync(name, lockValue, duration, CommandFlags.DemandMaster);

    public Task<bool> LockReleaseAsync(string name, string lockValue, int databaseId = -1)
    => Connection.GetDatabase(databaseId).LockReleaseAsync(name, lockValue, CommandFlags.DemandMaster);

    public async Task<bool> LockExtendAsync(string name, string lockValue, TimeSpan duration, int databaseId = -1)
        => await Connection.GetDatabase(databaseId).LockExtendAsync(name, lockValue, duration, CommandFlags.DemandMaster);

    public async Task<string> WaitForLockAsync(string key, bool throwExceptionIfLockNotAcquired = true)
    {
        var lockValue = Guid.NewGuid().ToString();
        var totalTime = TimeSpan.Zero;
        var maxTime = TimeSpan.FromSeconds(DistLockAcquisitionTimeoutInSeconds);
        var expiration = TimeSpan.FromSeconds(DistLockMaxDurationInSeconds);
        var sleepTime = TimeSpan.FromMilliseconds(RandomNumber.Local.Next(50, 600));
        var lockAchieved = false;

        while (!lockAchieved && totalTime < maxTime)
        {
            lockAchieved = await LockTakeAsync(key, lockValue, expiration);
            if (lockAchieved)
            {
                System.Diagnostics.Debug.WriteLine($"REDIS: lock acquired {key}; owner={lockValue}");
                continue;
            }
            await Task.Delay(sleepTime);
            totalTime += sleepTime;
        }

        if (throwExceptionIfLockNotAcquired && !lockAchieved)
        {
            throw new Exception("Failed to get lock; " + key);
        }

        return lockValue;
    }

    public string WaitForLock(string key, bool throwExceptionIfLockNotAcquired = true)
    {
        var lockValue = Guid.NewGuid().ToString();
        var totalTime = TimeSpan.Zero;
        var maxTime = TimeSpan.FromSeconds(DistLockAcquisitionTimeoutInSeconds);
        var expiration = TimeSpan.FromSeconds(DistLockMaxDurationInSeconds);
        var sleepTime = TimeSpan.FromMilliseconds(RandomNumber.Local.Next(50, 600));
        var lockAchieved = false;

        while (!lockAchieved && totalTime < maxTime)
        {
            lockAchieved = LockTake(key, lockValue, expiration);
            if (lockAchieved)
            {
                System.Diagnostics.Debug.WriteLine($"REDIS: lock acquired {key}; owner={lockValue}");
                continue;
            }
            Thread.Sleep(sleepTime);
            totalTime += sleepTime;
        }

        if (throwExceptionIfLockNotAcquired && !lockAchieved)
        {
            throw new Exception("Failed to get lock; " + key);
        }

        return lockValue;
    }

    public void Flush(int databaseId = 0) => Connection.GetEndPoints().ToList().ForEach(x => Connection.GetServer(x).FlushDatabase(databaseId));

    public async Task FlushAsync(int databaseId = 0)
    {
        var tasks = Connection.GetEndPoints().ToList().Select(x => Connection.GetServer(x).FlushDatabaseAsync(databaseId));
        await Task.WhenAll(tasks);
    }

    public void AssertLockRelease(string name, string lockValue) => Guard.IsTrue(LockRelease(name, lockValue), () => new Exception($"The lock '{name}' was not released"));

    public async Task AssertLockReleaseAsync(string name, string lockValue) => Guard.IsTrue(await LockReleaseAsync(name, lockValue), () => new Exception($"The lock '{name}' was not released"));
}
