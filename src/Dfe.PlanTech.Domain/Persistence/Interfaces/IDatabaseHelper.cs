using System.Linq.Expressions;
using System.Reflection;

namespace Dfe.PlanTech.Domain.Persistence.Interfaces;

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
    /// Gets an IQueryable for a specific entity type, and.
    /// </summary>
    /// <typeparam name="TDbEntity">The type of the database entity.</typeparam>
    /// <returns>An IQueryable for the specified entity type without auto-includes.</returns>
    public IQueryable<TDbEntity> GetQueryableForEntityExcludingAutoIncludesAndFilters<TDbEntity>() where TDbEntity : class;
    public IQueryable<TDbEntity> GetQueryableForEntityExcludingAutoIncludesAndFilters<TDbEntity>(TDbEntity entity) where TDbEntity : class;
    public IQueryable<TDbEntity> Include<TDbEntity, TProperty>(IQueryable<TDbEntity> queryable, Expression<Func<TDbEntity, TProperty>> expression) where TDbEntity : class;
    public void Add<TDbEntity>(TDbEntity entity) where TDbEntity : class;
    public void Update<TDbEntity>(TDbEntity entity) where TDbEntity : class;
    public void Remove<TDbEntity>(TDbEntity entity) where TDbEntity : class;
    public IEnumerable<PropertyInfo> GetRequiredPropertiesForType(Type type);
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
