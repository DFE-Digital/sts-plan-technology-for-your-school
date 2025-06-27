namespace Dfe.PlanTech.Core.Interfaces;

/// <summary>
/// Represents a distributed cache interface for managing cache items.
/// </summary>
public interface IDistributedCache
{
    /// <summary>
    /// Gets or creates a cache item asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the cache item.</typeparam>
    /// <param name="key">The key of the cache item.</param>
    /// <param name="action">The function to create the cache item if it does not exist.</param>
    /// <param name="expiry">The optional expiration time for the cache item.</param>
    /// <param name="onCacheItemCreation">An optional callback to invoke after the cache item is created.</param>
    /// <param name="databaseId">The optional database identifier.</param>
    /// <returns>The cached item or default value if not found.</returns>
    Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> action, TimeSpan? expiry = null, Func<T, Task>? onCacheItemCreation = null, int databaseId = -1);

    /// <summary>
    /// Sets a cache item asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the cache item.</typeparam>
    /// <param name="key">The key of the cache item.</param>
    /// <param name="value">The value of the cache item.</param>
    /// <param name="expiry">The optional expiration time for the cache item.</param>
    /// <param name="databaseId">The optional database identifier.</param>
    /// <returns>The key of the cache item.</returns>
    Task<string> SetAsync<T>(string key, T value, TimeSpan? expiry = null, int databaseId = -1);

    /// <summary>
    /// Removes a cache item asynchronously.
    /// </summary>
    /// <param name="key">The key of the cache item.</param>
    /// <param name="databaseId">The optional database identifier.</param>
    /// <returns>True if the item was removed; otherwise, false.</returns>
    Task<bool> RemoveAsync(string key, int databaseId = -1);

    /// <summary>
    /// Removes multiple cache items asynchronously.
    /// </summary>
    /// <param name="keys">The keys of the cache items.</param>
    Task RemoveAsync(params string[] keys);

    /// <summary>
    /// Removes multiple cache items from a specified database asynchronously.
    /// </summary>
    /// <param name="databaseId">The database identifier.</param>
    /// <param name="keys">The keys of the cache items.</param>
    Task RemoveAsync(int databaseId, params string[] keys);

    /// <summary>
    /// Appends an item to a cache entry asynchronously.
    /// </summary>
    /// <param name="key">The key of the cache item.</param>
    /// <param name="item">The item to append.</param>
    /// <param name="databaseId">The optional database identifier.</param>
    Task AppendAsync(string key, string item, int databaseId = -1);

    /// <summary>
    /// Retrieves a cache item asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the cache item.</typeparam>
    /// <param name="key">The key of the cache item.</param>
    /// <param name="databaseId">The optional database identifier.</param>
    /// <returns>The cached item or default value if not found.</returns>
    Task<T?> GetAsync<T>(string key, int databaseId = -1);

    /// <summary>
    /// Adds an item to a set in the cache asynchronously.
    /// </summary>
    /// <param name="key">The key of the set.</param>
    /// <param name="item">The item to add.</param>
    /// <param name="databaseId">The optional database identifier.</param>
    Task SetAddAsync(string key, string item, int databaseId = -1);

    /// <summary>
    /// Gets the members of a set from the cache asynchronously.
    /// </summary>
    /// <param name="key">The key of the set.</param>
    /// <param name="databaseId">The optional database identifier.</param>
    /// <returns>An array of set members.</returns>
    Task<IEnumerable<string>> GetSetMembersAsync(string key, int databaseId = -1);

    /// <summary>
    /// Removes an item from a set in the cache asynchronously.
    /// </summary>
    /// <param name="key">The key of the set.</param>
    /// <param name="item">The item to remove.</param>
    /// <param name="databaseId">The optional database identifier.</param>
    Task SetRemoveAsync(string key, string item, int databaseId = -1);

    /// <summary>
    /// Removes multiple items from a set in the cache asynchronously.
    /// </summary>
    /// <param name="key">The key of the set.</param>
    /// <param name="items">The items to remove.</param>
    /// <param name="databaseId">The optional database identifier.</param>
    Task SetRemoveItemsAsync(string key, IEnumerable<string> items, int databaseId = -1);
}
