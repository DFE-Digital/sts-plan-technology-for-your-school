namespace Dfe.PlanTech.Application.Caching.Interfaces;

/// <summary>
/// Represents a CMS specifc cache interface for managing cache items, their dependencies, and cache invalidation.
/// </summary>
public interface ICmsCache : IDistributedCache
{
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
