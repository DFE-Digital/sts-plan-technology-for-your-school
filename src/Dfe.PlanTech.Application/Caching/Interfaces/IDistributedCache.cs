namespace Dfe.PlanTech.Application.Caching.Interfaces;

public interface IDistributedCache
{
    Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> action, TimeSpan? expiry = null, Func<T, Task>? onCacheItemCreation = null, int databaseId = -1);
    Task<string> SetAsync<T>(string key, T value, TimeSpan? expiry = null, int databaseId = -1);
    Task<bool> RemoveAsync(string key, int databaseId = -1);
    Task RemoveAsync(params string[] keys);
    Task RemoveAsync(int databaseId, params string[] keys);
    Task AppendAsync(string key, string item, int databaseId = -1);
    Task<T?> GetAsync<T>(string key, int databaseId = -1);
    Task SetAddAsync(string key, string item, int databaseId = -1);
    Task<string[]> GetSetMembersAsync(string key, int databaseId = -1);
    Task SetRemoveAsync(string key, string item, int databaseId = -1);
    Task SetRemoveItemsAsync(string key, string[] items, int databaseId = -1);
}
