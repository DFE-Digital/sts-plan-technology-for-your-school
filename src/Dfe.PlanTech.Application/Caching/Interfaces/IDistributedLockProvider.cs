namespace Dfe.PlanTech.Application.Caching.Interfaces;

public interface IDistributedLockProvider
{
    Task<bool> LockReleaseAsync(string key, string lockValue, int databaseId = -1);
    Task<bool> LockExtendAsync(string key, string lockValue, TimeSpan duration, int databaseId = -1);
    Task<string> WaitForLockAsync(string key, bool throwExceptionIfLockNotAcquired = true);
    Task LockAndRun(string key, Func<Task> runWithLock, int databaseId = -1);
    Task<T?> LockAndGet<T>(string key, Func<Task<T>> runWithLock, int databaseId = -1);
}
