namespace Dfe.PlanTech.Core.Caching.Interfaces;

/// <summary>
/// Represents a CMS specifc cache interface for managing cache items, their dependencies, and cache invalidation.
/// </summary>
public interface ICmsCache : IDistributedCache
{
    /// <summary>
    /// Iterates through all items in a dependency array and removes them from the cache
    /// Then removes the dependency array itself
    /// </summary>
    /// <param name="contentComponentId">Id of component to invalidate dependencies of</param>
    /// <param name="contentType">Name of the content type of the component being invalidated</param>
    /// <returns></returns>
    Task InvalidateCacheAsync(string contentComponentId, string contentType);
}
