using System.Linq.Expressions;
using System.Reflection;

namespace Dfe.PlanTech.Application.Persistence.Interfaces;

/// <summary>
/// Provides abstractions for interfacing with the EFCore DB without having to add the EF Core packages directly.
/// </summary>
/// <typeparam name="TIDbContext">The type of DbContext.</typeparam>
public interface IDatabaseHelper<TIDbContext> where TIDbContext : IDbContext
{
    public TIDbContext Database { get; }

    /// <summary>
    /// Clears the change tracker for the current DbContext.
    /// </summary>
    public void ClearTracking();

    public IQueryable<TDbEntity> GetIQueryableForEntity<TDbEntity>() where TDbEntity : class;

    /// <summary>
    /// Gets an IQueryable for a specific entity type, whilst ignoring any existing AutoInclude and QueryFilters
    /// </summary>
    /// <typeparam name="TDbEntity">The type of the database entity.</typeparam>
    /// <returns>An IQueryable for the specified entity type without auto-includes.</returns>
    public IQueryable<TDbEntity> GetQueryableForEntityExcludingAutoIncludesAndFilters<TDbEntity>() where TDbEntity : class;

    /// <summary>
    /// Gets an IQueryable for a specific entity type, whilst ignoring any existing AutoInclude and QueryFilters
    /// </summary>
    /// <typeparam name="TDbEntity">The type of the database entity.</typeparam>
    /// <returns>An IQueryable for the specified entity type without auto-includes.</returns>
    public IQueryable<TDbEntity> GetQueryableForEntityExcludingAutoIncludesAndFilters<TDbEntity>(TDbEntity entity) where TDbEntity : class;

    /// <summary>
    /// Include a given reference when querying the DB
    /// </summary>
    /// <typeparam name="TDbEntity">The type of the database entity.</typeparam>
    /// <typeparam name="TProperty">The type of the property/reference we are including.</typeparam>
    /// <param name="queryable">The existing IQueryable on which to include references</param>
    /// <param name="expression">Expression to select what references/property we are including</param>
    /// <returns></returns>
    public IQueryable<TDbEntity> Include<TDbEntity, TProperty>(IQueryable<TDbEntity> queryable, Expression<Func<TDbEntity, TProperty>> expression) where TDbEntity : class;

    /// <summary>
    /// Adds the entity to the DB. Does not save - requires executing SaveChanges/SaveChangesAsync
    /// </summary>
    /// <typeparam name="TDbEntity"></typeparam>
    /// <param name="entity"></param>
    public void Add<TDbEntity>(TDbEntity entity) where TDbEntity : class;

    /// <summary>
    /// Updates the entity in the DB. Does not save - requires executing SaveChanges/SaveChangesAsync
    /// </summary>
    /// <typeparam name="TDbEntity"></typeparam>
    /// <param name="entity"></param>
    public void Update<TDbEntity>(TDbEntity entity) where TDbEntity : class;

    /// <summary>
    /// Removes the entity from the DB. Does not save - requires executing SaveChanges/SaveChangesAsync
    /// </summary>
    /// <typeparam name="TDbEntity"></typeparam>
    /// <param name="entity"></param>
    public void Remove<TDbEntity>(TDbEntity entity) where TDbEntity : class;

    /// <summary>
    /// Get all properties of the entity in the DB that are non-nullable
    /// </summary>
    /// <param name="type">The type to get non-nullable properties from</param>
    /// <returns></returns>
    public IEnumerable<PropertyInfo> GetRequiredPropertiesForType(Type type);

    /// <summary>
    /// Saves all tracked changes in the DBs
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
