using Dfe.PlanTech.Domain.Caching.Interfaces;

namespace Dfe.PlanTech.Application.Caching.Models;

public class DisabledCacheFactory : ICachedFactory
{
    public T GetOrCreate<T>(string key, Func<T> action, TimeSpan? expiry = null, Action<T>? onCacheItemCreation = null, int databaseId = -1) => action();

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> action, TimeSpan? expiry = null, Func<T, Task>? onCacheItemCreation = null, int databaseId = -1)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var rv = await action();
        sw.Stop();
        return rv;
    }
}
