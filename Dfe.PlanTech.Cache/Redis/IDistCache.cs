namespace Dfe.PlanTech.Cache.Redis;

public interface IDistCache : ICachedFactory
{
    Task InitialiseAsync();
    void Append(string key, string item, int databaseId = -1);
    void AssertLockRelease(string name, LockOwner lockOwner);
    Task AssertLockReleaseAsync(string name, LockOwner lockOwner);
    void Flush(int databaseId = 0);
    Task FlushAsync(int databaseId = 0);
    T Get<T>(string key, int databaseId = -1);
    Task<T> GetAsync<T>(string key, int databaseId = -1);
    bool LockExtend(string name, LockOwner lockOwner, TimeSpan duration, int databaseId = -1);
    Task<bool> LockExtendAsync(string name, LockOwner lockOwner, TimeSpan duration, int databaseId = -1);
    bool LockRelease(string name, LockOwner lockOwner, int databaseId = -1);
    Task<bool> LockReleaseAsync(string name, LockOwner lockOwner, int databaseId = -1);
    bool LockTake(string name, LockOwner lockOwner, TimeSpan duration, int databaseId = -1);
    Task<bool> LockTakeAsync(string name, LockOwner lockOwner, TimeSpan duration, int databaseId = -1);
    void Remove(string key, int databaseId = -1);
    void Remove(string[] keys, int databaseId = -1);
    Task<bool> RemoveAsync(string key, int databaseId = -1);
    Task RemoveAsync(int databaseId = -1, params string[] keys);
    Task RemoveAsync(params string[] keys);
    void SetAdd(string key, string item, int databaseId = -1);
    Task SetAddAsync(string key, string item, int databaseId = -1);
    Task<string> SetAsync<T>(string key, T value, TimeSpan? expiry = null, int databaseId = -1);
    string[] GetSetMembers(string key, int databaseId = -1);
    Task<string[]> GetSetMembersAsync(string key, int databaseId = -1);
    void SetRemove(string key, string item, int databaseId = -1);
    void SetRemove(string key, string[] items, int databaseId = -1);
    LockOwner WaitForLock(object key, bool throwExceptionIfLockNotAcquired = true);
    Task<LockOwner> WaitForLockAsync(object key, bool throwExceptionIfLockNotAcquired = true);
}
