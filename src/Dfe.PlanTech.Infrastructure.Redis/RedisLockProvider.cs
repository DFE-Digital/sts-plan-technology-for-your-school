using Dfe.PlanTech.Core.Caching;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Helpers;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Dfe.PlanTech.Infrastructure.Redis;

public class RedisLockProvider(
    ILogger<RedisLockProvider> logger,
    IRedisConnectionManager connectionManager,
    DistributedCachingOptions options
) : IDistributedLockProvider
{
    private readonly ILogger<RedisLockProvider> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IRedisConnectionManager _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
    private readonly DistributedCachingOptions _options = options ?? throw new ArgumentNullException(nameof(options));

    /// <inheritdoc />
    public async Task<bool> LockReleaseAsync(string key, string lockValue, int databaseId = -1)
    {
        _logger.LogInformation("Releasing lock for key: {Key} with lock value: {LockValue}", key, lockValue);
        var database = await _connectionManager.GetDatabaseAsync(databaseId);
        return await database.LockReleaseAsync(key, lockValue, CommandFlags.DemandMaster);
    }

    /// <inheritdoc />
    public async Task<bool> LockExtendAsync(string key, string lockValue, TimeSpan duration, int databaseId = -1)
    {
        _logger.LogInformation("Extending lock for key: {Key} with lock value: {LockValue} for duration: {Duration}", key, lockValue, duration);
        var database = await _connectionManager.GetDatabaseAsync(databaseId);
        return await database.LockExtendAsync(key, lockValue, duration, CommandFlags.DemandMaster);
    }

    /// <inheritdoc />
    public async Task<string?> WaitForLockAsync(string key, bool throwExceptionIfLockNotAcquired = true)
    {
        var lockValue = Guid.NewGuid().ToString();
        var totalTime = TimeSpan.Zero;
        var maxTime = TimeSpan.FromSeconds(_options.DistLockMaxDurationInSeconds);
        var expiration = TimeSpan.FromSeconds(_options.DistLockAcquisitionTimeoutInSeconds);
        var sleepTime = TimeSpan.FromMilliseconds(RandomNumberHelper.GenerateRandomInt(50, 600));

        _logger.LogInformation("Attempting to acquire lock for key: {Key}", key);

        while (totalTime < maxTime)
        {
            if (await LockTakeAsync(key, lockValue, expiration))
            {
                _logger.LogInformation("Lock acquired for key: {Key} with lock value: {LockValue}", key, lockValue);
                return lockValue;
            }

            _logger.LogWarning("Lock not acquired for key: {Key}, retrying...", key);
            await Task.Delay(sleepTime);
            totalTime += sleepTime;
        }

        if (throwExceptionIfLockNotAcquired)
        {
            _logger.LogError("Failed to acquire lock for key: {Key}", key);
            throw new LockException($"Failed to get lock for: {key}");
        }

        return null;
    }

    /// <inheritdoc />
    public async Task LockAndRun(string key, Func<Task> runWithLock, int databaseId = -1)
    {
        string? lockValue = await WaitForLockAsync(key, true);
        if (string.IsNullOrEmpty(lockValue))
            return;

        try
        {
            await runWithLock();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while executing locked operation for key: {Key}", key);
        }
        finally
        {
            await LockReleaseAsync(key, lockValue, databaseId);
        }
    }

    /// <inheritdoc />
    public async Task<T?> LockAndGet<T>(string key, Func<Task<T>> runWithLock, int databaseId = -1)
    {
        string? lockValue = await WaitForLockAsync(key, true);
        if (string.IsNullOrEmpty(lockValue))
            return default;

        T? result = default;

        try
        {
            result = await runWithLock();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while executing locked operation for key: {Key}", key);
        }
        finally
        {
            await LockReleaseAsync(key, lockValue, databaseId);
        }

        return result;
    }

    /// <summary>
    /// Attempts to take a distributed lock.
    /// </summary>
    /// <param name="key">The key associated with the lock.</param>
    /// <param name="lockValue">The lock value used to acquire the lock.</param>
    /// <param name="duration">The duration for which the lock should be held.</param>
    /// <param name="databaseId">The Redis database ID to use. Defaults to -1, which means using the default database.</param>
    /// <returns>True if the lock was successfully taken; otherwise, false.</returns>
    private async Task<bool> LockTakeAsync(string key, string lockValue, TimeSpan duration, int databaseId = -1)
    {
        _logger.LogInformation("Taking lock for key: {Key} with lock value: {LockValue} for duration: {Duration}", key, lockValue, duration);
        var database = await _connectionManager.GetDatabaseAsync(databaseId);
        return await database.LockTakeAsync(key, lockValue, duration);
    }
}
