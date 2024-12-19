using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Persistence.Interfaces;

namespace Dfe.PlanTech.Application.Persistence.Interfaces;

/// <summary>
/// Abstraction around repositories used for retrieving Content
/// </summary>
public interface IContentRepository
{
    /// <summary>
    /// Get an entity by Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="include">Depth of references to include</param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    Task<TEntity?> GetEntityById<TEntity>(string id, int include = 2, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get options to use for fetching an Entity by Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="include"></param>
    /// <returns></returns>
    GetEntitiesOptions GetEntityByIdOptions(string id, int include = 2);

    /// <summary>
    /// Get all entities of the specified type, using the name of the generic parameter's type as the entity type id (to lower case).
    /// E.g. if the TEntity is a class called "Category", then it uses "category"
    /// </summary>
    /// <param name="options">Filtering options</param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    Task<IEnumerable<TEntity>> GetEntities<TEntity>(IGetEntitiesOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get entities without filtering
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    Task<IEnumerable<TEntity>> GetEntities<TEntity>(CancellationToken cancellationToken = default);
}
