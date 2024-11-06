namespace Dfe.PlanTech.Application.Caching.Interfaces;

/// <summary>
/// Represents a CMS specifc cache interface for managing cache items, their dependencies, and cache invalidation.
/// </summary>
public interface ICmsCache : IDistributedCache
{
    /// <summary>
    /// Marks the key as a dependency of all the ContentIds and Slugs within the cached body
    /// </summary>
    /// <param name="key">Key of the cache item</param>
    /// <param name="value">value being stored</param>
    /// <returns></returns>
    Task RegisterDependenciesAsync<T>(string key, T value);

    /// <summary>
    /// Iterates through all items in a dependency array and removes them from the cache
    /// Then removes the dependency array itself
    /// </summary>
    /// <param name="contentComponentId">id of component to invalidate dependencies of</param>
    /// <returns></returns>
    Task InvalidateCacheAsync(string contentComponentId);

    /// <summary>
    /// Generates a redis key for a content component dependency
    /// </summary>
    /// <param name="contentComponentId"></param>
    /// <returns></returns>
    string GetDependencyKey(string contentComponentId);
}
