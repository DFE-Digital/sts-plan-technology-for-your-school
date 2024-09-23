using System.Linq.Expressions;
using System.Reflection;

namespace Dfe.PlanTech.Application.Persistence.Interfaces;

public interface IDatabaseHelper<TIDbContext> where TIDbContext : IDbContext
{
  public TIDbContext Database { get; }
  public void ClearTracking();
  public IQueryable<TDbEntity> GetIQueryableForEntity<TDbEntity>() where TDbEntity : class;
  public IQueryable<TDbEntity> GetIQueryableForEntityWithoutAutoIncludes<TDbEntity>() where TDbEntity : class;
  public IQueryable<TDbEntity> GetIQueryableForEntityWithoutAutoIncludes<TDbEntity>(TDbEntity entity) where TDbEntity : class;
  public IQueryable<TDbEntity> Include<TDbEntity, TProperty>(IQueryable<TDbEntity> queryable, Expression<Func<TDbEntity, TProperty>> expression) where TDbEntity : class;
  public void Add<TDbEntity>(TDbEntity entity) where TDbEntity : class;
  public void Update<TDbEntity>(TDbEntity entity) where TDbEntity : class;
  public void Remove<TDbEntity>(TDbEntity entity) where TDbEntity : class;
  public IEnumerable<PropertyInfo> GetRequiredPropertiesForType(Type type);
  public Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}