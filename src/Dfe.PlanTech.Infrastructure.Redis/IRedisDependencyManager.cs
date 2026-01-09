using StackExchange.Redis;

namespace Dfe.PlanTech.Infrastructure.Redis;

/// <summary>
/// Manages Redis dependencies for <see cref="IContentComponent"> and their contents.
/// </summary>
public interface IRedisDependencyManager
{
    /// <summary>
    /// Key to use for dependencies of a content fetch that returns nothing
    /// </summary>
    string EmptyCollectionDependencyKey { get; }

    /// <summary>
    /// Find and set dependencies for a given <see cref="IContentComponent"/>
    /// </summary>
    /// <typeparam name="T">Type of the value to register as a dependency.</typeparam>
    /// <param name="database">The database where dependencies are stored.</param>
    /// <param name="key">Key for the parent of the dependencies.</param>
    /// <param name="value">The <see cref="IContentComponent"/> parent of the dependencies </param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RegisterDependenciesAsync<T>(
        IDatabase database,
        string key,
        T value,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Generates a key for the given content component ID.
    /// </summary>
    /// <param name="contentComponentId">The ID of the content component.</param>
    /// <returns>The generated key string.</returns>
    /// <example>
    /// <code>
    /// var contentComponentId = "example_id";
    /// var dependencyKey = GetDependencyKey(contentComponentId);
    /// Console.WriteLine(dependencyKey); // Output: "dependency:example_id"
    /// </code>
    /// </example>
    string GetDependencyKey(string contentComponentId);
}
