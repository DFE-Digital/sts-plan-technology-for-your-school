namespace Dfe.PlanTech.Infrastructure.Redis;

/// <summary>
/// Provides distributed locking functionality
/// </summary>
public interface IDistributedLockProvider
{
    /// <summary>
    /// Releases the lock for the specified key and lock value
    /// </summary>
    /// <param name="key">The key associated with the lock.</param>
    /// <param name="lockValue">The lock value used to acquire the lock.</param>
    /// <param name="databaseId">The Redis database ID to use. Defaults to -1, which means using the default database.</param>
    /// <returns>True if the lock was successfully released; otherwise, false.</returns>
    Task<bool> LockReleaseAsync(string key, string lockValue, int databaseId = -1);

    /// <summary>
    /// Extends the expiration of a distributed lock for the specified key and lock value.
    /// </summary>
    /// <param name="key">The key associated with the lock.</param>
    /// <param name="lockValue">The lock value used to acquire the lock.</param>
    /// <param name="duration">The new duration to set for the lock's expiration.</param>
    /// <param name="databaseId">The Redis database ID to use. Defaults to -1, which means using the default database.</param>
    /// <returns>True if the lock was successfully extended; otherwise, false.</returns>
    Task<bool> LockExtendAsync(
        string key,
        string lockValue,
        TimeSpan duration,
        int databaseId = -1
    );

    /// <summary>
    /// Waits for a distributed lock to be acquired.
    /// </summary>
    /// <param name="key">The key associated with the lock.</param>
    /// <param name="throwExceptionIfLockNotAcquired">Indicates whether an exception should be thrown if the lock is not acquired.</param>
    /// <returns>The lock value if the lock was acquired; otherwise, null.</returns>
    Task<string?> WaitForLockAsync(string key, bool throwExceptionIfLockNotAcquired = true);

    /// <summary>
    /// Executes an operation with a distributed lock.
    /// </summary>
    /// <param name="key">The key associated with the lock.</param>
    /// <param name="runWithLock">The action to execute while holding the lock.</param>
    /// <param name="databaseId">The Redis database ID to use. Defaults to -1, which means using the default database.</param>
    Task LockAndRun(string key, Func<Task> runWithLock, int databaseId = -1);

    /// <summary>
    /// Executes an operation with a distributed lock and returns the result.
    /// </summary>
    /// <param name="key">The key associated with the lock.</param>
    /// <param name="runWithLock">The function to execute while holding the lock.</param>
    /// <param name="databaseId">The Redis database ID to use. Defaults to -1, which means using the default database.</param>
    /// <returns>The result of the operation if the lock was acquired; otherwise, the default value.</returns>
    Task<T?> LockAndGet<T>(string key, Func<Task<T>> runWithLock, int databaseId = -1);
}
