using Dfe.PlanTech.Domain.Background;
using Dfe.PlanTech.Domain.Content.Interfaces;
using StackExchange.Redis;

namespace Dfe.PlanTech.Infrastructure.Redis;

/// <inheritdoc cref="IRedisDependencyManager"/>
/// <param name="backgroundTaskQueue">To add dependency set operations to a queue for background processing</param>
public class RedisDependencyManager(IBackgroundTaskQueue backgroundTaskQueue) : IRedisDependencyManager
{
    public string EmptyCollectionDependencyKey => "Missing";

    /// <inheritdoc cref="IRedisDependencyManager"/>
    public Task RegisterDependenciesAsync<T>(IDatabase database, string key, T value, CancellationToken cancellationToken = default)
    => backgroundTaskQueue.QueueBackgroundWorkItemAsync((cancellationToken) => GetAndSetDependencies(database, key, value));

    /// <inheritdoc cref="IRedisDependencyManager"/>
    public string GetDependencyKey(string contentComponentId) => $"Dependency:{contentComponentId}";

    /// <summary>
    /// Retrieves dependencies for the given <see cref="IContentComponent"/> and registers them in the Redis cache.
    /// </summary>
    /// <typeparam name="T">Type of the value whose dependencies are to be retrieved.</typeparam>
    /// <param name="database">The database where dependencies are stored.</param>
    /// <param name="key">Key for the parent of the dependencies.</param>
    /// <param name="value">The <see cref="IContentComponent"/> parent of the dependencies.</param>
    private async Task GetAndSetDependencies<T>(IDatabase database, string key, T value)
    {
        var batch = database.CreateBatch();
        var tasks = GetDependencies(value).Select(dependency => batch.SetAddAsync(GetDependencyKey(dependency), key, CommandFlags.FireAndForget)).ToArray();
        if (tasks.Length == 0)
        {
            tasks = tasks.Append(batch.SetAddAsync(EmptyCollectionDependencyKey, key, CommandFlags.FireAndForget)).ToArray();
        }
        batch.Execute();
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Retrieves dependencies, in the form of the Id of the <see cref="IContentComponent"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Thrown when the value object is not a <see cref="IContentComponent"/> or <see cref="IEnumerable{IContentComponent}"/> </exception>
    private IEnumerable<string> GetDependencies<T>(T? value)
    => value switch
    {
        null => [],
        IEnumerable<IContentComponent> collection => collection.SelectMany(GetDependencies),
        IContentComponent item => GetContentDependenciesAsync(item),
        _ => throw new InvalidOperationException($"{value!.GetType()} is not a {typeof(IContentComponent)} or a {typeof(IEnumerable<IContentComponent>)}"),
    };

    /// <summary>
    /// Uses reflection to check for any ContentIds within the <see cref="IContentComponent">, and returns the Id value of any found
    /// </summary>
    /// <param name="value"></param>
    private IEnumerable<string> GetContentDependenciesAsync(IContentComponent value)
    {
        // RichText is a sub-component that doesn't have SystemDetails, exit for such types
        if (value.Sys is null)
            yield break;

        yield return value.Sys.Id;
        var properties = value.GetType().GetProperties();
        foreach (var property in properties)
        {
            if (typeof(IContentComponent).IsAssignableFrom(property.PropertyType) || typeof(IEnumerable<IContentComponent>).IsAssignableFrom(property.PropertyType))
            {
                foreach (var dependency in GetDependencies(property.GetValue(value)))
                {
                    yield return dependency;
                }
            }
        }
    }
}
