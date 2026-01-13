using Dfe.PlanTech.Core.Contentful.Options;

namespace Dfe.PlanTech.Data.Contentful.Interfaces;

/// <summary>
/// Abstraction around repositories used for retrieving Content
/// </summary>
public interface IContentfulRepository
{
    /// <summary>
    /// Get an entity by Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="include">Depth of references to include</param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntry"></typeparam>
    /// <returns></returns>
    Task<TEntry?> GetEntryByIdAsync<TEntry>(string id, int include = 2);

    /// <summary>
    /// Get options to use for fetching an Entry by Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="include"></param>
    /// <returns></returns>
    GetEntriesOptions GetEntryByIdOptions(string id, int include = 2);

    /// <summary>
    /// Get all entities of the specified type, using the name of the generic parameter's type as the entity type id (to lower case).
    /// E.g. if the TEntry is a class called "Category", then it uses "category"
    /// </summary>
    /// <param name="options">Filtering options</param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntry"></typeparam>
    /// <returns></returns>
    Task<IEnumerable<TEntry>> GetEntriesAsync<TEntry>(GetEntriesOptions options);

    /// <summary>
    /// Get entities without filtering
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntry"></typeparam>
    /// <returns></returns>
    Task<IEnumerable<TEntry>> GetEntriesAsync<TEntry>();

    Task<IEnumerable<TEntry>> GetPaginatedEntriesAsync<TEntry>(GetEntriesOptions options);

    Task<int> GetEntriesCountAsync<TEntry>();
}
