using System.Linq.Expressions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Application.Persistence.Extensions;

public static class IDatabaseHelperExtensions
{

  public static IQueryable<TDbEntity?> EntitiesMatchingId<TDbEntity, TDbContext>(this IDatabaseHelper<TDbContext> databaseHelper, TDbEntity entity)
    where TDbEntity : ContentComponentDbEntity
    where TDbContext : IDbContext
      => databaseHelper.GetIQueryableForEntityWithoutAutoIncludes(entity).Where(entity => entity.Id == entity.Id);

  public static async Task<TDbEntity?> GetMatchingEntityById<TDbEntity, TDbContext>(this IDatabaseHelper<TDbContext> databaseHelper, TDbEntity entity, CancellationToken cancellationToken)
    where TDbEntity : ContentComponentDbEntity
    where TDbContext : IDbContext
      => await databaseHelper.Database.FirstOrDefaultAsync(databaseHelper.EntitiesMatchingId(entity), cancellationToken);

  public static IQueryable<TDbEntity> Include<TDbEntity, TProperty, TDbContext>(this IQueryable<TDbEntity> queryable, Expression<Func<TDbEntity, TProperty>> expression, IDatabaseHelper<TDbContext> databaseHelper)
  where TDbEntity : class
  where TDbContext : IDbContext
  => databaseHelper.Include(queryable, expression);

  public static Task<List<TDbEntity>> ToListAsync<TDbEntity, TDbContext>(this IQueryable<TDbEntity> queryable, IDatabaseHelper<TDbContext> databaseHelper, CancellationToken cancellationToken)
  where TDbEntity : class
  where TDbContext : IDbContext
  => databaseHelper.Database.ToListAsync(queryable, cancellationToken);

}