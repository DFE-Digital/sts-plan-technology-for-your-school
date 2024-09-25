using System.Linq.Expressions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Application.Persistence.Extensions;

/// <summary>
/// Various extensions methods for repeating various operations against a DbContext using the <see cref="IDatabaseHelper{TIDbContext}"/> class
/// </summary>
public static class IDatabaseHelperExtensions
{
    /// <summary>
    /// Gets the matching existing entity from the database
    /// </summary>
    /// <typeparam name="TDbEntity">The type of the database entity.</typeparam>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    /// <param name="databaseHelper"></param>
    /// <param name="entity">The entity to match.</param>
    /// <returns>An <see cref="IQueryable{TDbEntity?}"/> of entities matching the specified entity.</returns>
    public static Task<TDbEntity?> GetMatchingEntityById<TDbEntity, TDbContext>(this IDatabaseHelper<TDbContext> databaseHelper, TDbEntity entity, CancellationToken cancellationToken)
      where TDbEntity : ContentComponentDbEntity
      where TDbContext : IDbContext
        =>  databaseHelper.Database.FirstOrDefaultAsync(databaseHelper.EntitiesMatchingId(entity), cancellationToken);

    /// <summary>
    /// Include a related entity from the database when retrieved
    /// </summary>
    /// <typeparam name="TDbEntity"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    /// <typeparam name="TDbContext"></typeparam>
    /// <param name="queryable"></param>
    /// <param name="expression">Related property selector</param>
    /// <param name="databaseHelper"></param>
    /// <returns></returns>
    public static IQueryable<TDbEntity> Include<TDbEntity, TProperty, TDbContext>(this IQueryable<TDbEntity> queryable, Expression<Func<TDbEntity, TProperty>> expression, IDatabaseHelper<TDbContext> databaseHelper)
      where TDbEntity : class
      where TDbContext : IDbContext
        => databaseHelper.Include(queryable, expression);

    public static Task<List<TDbEntity>> ToListAsync<TDbEntity, TDbContext>(this IQueryable<TDbEntity> queryable, IDatabaseHelper<TDbContext> databaseHelper, CancellationToken cancellationToken)
      where TDbEntity : class
      where TDbContext : IDbContext
        => databaseHelper.Database.ToListAsync(queryable, cancellationToken);

    /// <summary>
    /// Gets an <see cref="IQueryable{TDbEntity?}"/> of entities matching the specified entity.
    /// </summary>
    /// <remarks>
    /// This method isn't really needed, but it helps keep GetMatchingEntityById method a bit cleaner
    /// </remarks>
    /// <typeparam name="TDbEntity">The type of the database entity.</typeparam>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    /// <param name="databaseHelper"></param>
    /// <param name="entity">The entity to match.</param>
    /// <returns>An <see cref="IQueryable{TDbEntity?}"/> of entities matching the specified entity.</returns>
    public static IQueryable<TDbEntity?> EntitiesMatchingId<TDbEntity, TDbContext>(this IDatabaseHelper<TDbContext> databaseHelper, TDbEntity entity)
      where TDbEntity : ContentComponentDbEntity
      where TDbContext : IDbContext
        => databaseHelper.GetQueryableForEntityExcludingAutoIncludesAndFilters(entity).Where(existing => existing.Id == entity.Id);
}
