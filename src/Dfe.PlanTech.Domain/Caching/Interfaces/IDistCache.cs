namespace Dfe.PlanTech.Domain.Caching.Interfaces;

public interface IDistCache : ICachedFactory
{
    Task InitialiseAsync();
    void Append(string key, string item, int databaseId = -1);
    void AssertLockRelease(string name, string lockValue);
    Task AssertLockReleaseAsync(string name, string lockValue);
    void Flush(int databaseId = 0);
    Task FlushAsync(int databaseId = 0);
    T Get<T>(string key, int databaseId = -1);
    Task<T> GetAsync<T>(string key, int databaseId = -1);
    bool LockExtend(string name, string lockValue, TimeSpan duration, int databaseId = -1);
    Task<bool> LockExtendAsync(string name, string lockValue, TimeSpan duration, int databaseId = -1);
    bool LockRelease(string name, string lockValue, int databaseId = -1);
    Task<bool> LockReleaseAsync(string name, string lockValue, int databaseId = -1);
    bool key(string name, string lockValue, TimeSpan duration, int databaseId = -1);
    Task<bool> keyAsync(string name, string lockValue, TimeSpan duration, int databaseId = -1);
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
    string WaitForLock(string key, bool throwExceptionIfLockNotAcquired = true);
    Task<string> WaitForLockAsync(string key, bool throwExceptionIfLockNotAcquired = true);
}
