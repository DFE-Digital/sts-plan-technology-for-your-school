namespace Dfe.PlanTech.Domain.Caching.Interfaces;

public interface ICachedFactory
{
    T GetOrCreate<T>(string key, Func<T> action, TimeSpan? expiry = null, Action<T>? onCacheItemCreation = null, int databaseId = -1);
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> action, TimeSpan? expiry = null, Func<T, Task>? onCacheItemCreation = null, int databaseId = -1);
}
