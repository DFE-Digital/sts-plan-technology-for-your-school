using Dfe.PlanTech.Core.Caching;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Helpers;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Dfe.PlanTech.Infrastructure.Redis;

public class RedisLockProvider(DistributedCachingOptions options, IRedisConnectionManager connectionManager, ILogger<RedisLockProvider> logger) : IDistributedLockProvider
{
    /// <inheritdoc />
    public async Task<bool> LockReleaseAsync(string key, string lockValue, int databaseId = -1)
    {
        logger.LogInformation("Releasing lock for key: {Key} with lock value: {LockValue}", key, lockValue);
        var database = await connectionManager.GetDatabaseAsync(databaseId);
        return await database.LockReleaseAsync(key, lockValue, CommandFlags.DemandMaster);
    }

    /// <inheritdoc />
    public async Task<bool> LockExtendAsync(string key, string lockValue, TimeSpan duration, int databaseId = -1)
    {
        logger.LogInformation("Extending lock for key: {Key} with lock value: {LockValue} for duration: {Duration}", key, lockValue, duration);
        var database = await connectionManager.GetDatabaseAsync(databaseId);
        return await database.LockExtendAsync(key, lockValue, duration, CommandFlags.DemandMaster);
    }

    /// <inheritdoc />
    public async Task<string?> WaitForLockAsync(string key, bool throwExceptionIfLockNotAcquired = true)
    {
        var lockValue = Guid.NewGuid().ToString();
        var totalTime = TimeSpan.Zero;
        var maxTime = TimeSpan.FromSeconds(options.DistLockMaxDurationInSeconds);
        var expiration = TimeSpan.FromSeconds(options.DistLockAcquisitionTimeoutInSeconds);
        var sleepTime = TimeSpan.FromMilliseconds(RandomNumberHelper.GenerateRandomInt(50, 600));

        logger.LogInformation("Attempting to acquire lock for key: {Key}", key);

        while (totalTime < maxTime)
        {
            if (await LockTakeAsync(key, lockValue, expiration))
            {
                logger.LogInformation("Lock acquired for key: {Key} with lock value: {LockValue}", key, lockValue);
                return lockValue;
            }

            logger.LogWarning("Lock not acquired for key: {Key}, retrying...", key);
            await Task.Delay(sleepTime);
            totalTime += sleepTime;
        }

        if (throwExceptionIfLockNotAcquired)
        {
            logger.LogError("Failed to acquire lock for key: {Key}", key);
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
            logger.LogError(ex, "Error while executing locked operation for key: {Key}", key);
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
            logger.LogError(ex, "Error while executing locked operation for key: {Key}", key);
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
        logger.LogInformation("Taking lock for key: {Key} with lock value: {LockValue} for duration: {Duration}", key, lockValue, duration);
        var database = await connectionManager.GetDatabaseAsync(databaseId);
        return await database.LockTakeAsync(key, lockValue, duration);
    }
}
