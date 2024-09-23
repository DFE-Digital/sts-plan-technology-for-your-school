using System.Linq.Expressions;
using System.Reflection;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.PlanTech.Infrastructure.Data;

public class DatabaseHelper<TIDbContext> : IDatabaseHelper<TIDbContext>
where TIDbContext : IDbContext
{
  private readonly DbContext concreteDb;
  private readonly TIDbContext abstractDb;

  protected DbContext Db => concreteDb;

  public TIDbContext Database => abstractDb;

  public DatabaseHelper(IServiceProvider services)
  {
    var (db, interfaceDb) = GetDbContext(services);
    concreteDb = db;
    abstractDb = interfaceDb;
  }

  public void ClearTracking()
  {
    Db.ChangeTracker.Clear();
  }

  public IQueryable<TDbEntity> GetIQueryableForEntity<TDbEntity>(IEntityType model)
    => (IQueryable<TDbEntity>)Db.GetType().GetMethod("Set", 1, Type.EmptyTypes)!.MakeGenericMethod(model!.ClrType)!.Invoke(concreteDb, null)!;

  public IQueryable<TDbEntity> GetIQueryableForEntity<TDbEntity>() where TDbEntity : class
  => GetIQueryableForEntity<TDbEntity>(GetEntityType<TDbEntity>());

  public IQueryable<TDbEntity> GetIQueryableForEntityWithoutAutoIncludes<TDbEntity>() where TDbEntity : class => GetDbSet<TDbEntity>().IgnoreAutoIncludes();
  public IQueryable<TDbEntity> GetIQueryableForEntityWithoutAutoIncludes<TDbEntity>(TDbEntity entity) where TDbEntity : class => GetDbSet<TDbEntity>().IgnoreAutoIncludes();

  public void Add<TDbEntity>(TDbEntity entity) where TDbEntity : class => Db.Add(entity);

  public void Update<TDbEntity>(TDbEntity entity) where TDbEntity : class => Db.Update(entity);

  public void Remove<TDbEntity>(TDbEntity entity) where TDbEntity : class => Db.Remove(entity);

  private static (DbContext, TIDbContext) GetDbContext(IServiceProvider services)
  {
    var dbContexts = GetDbContexts();
    var type = dbContexts.FirstOrDefault(context => context.IsAssignableTo(typeof(TIDbContext))) ?? throw new KeyNotFoundException($"Couldn't find DbContext inheriting {typeof(TIDbContext)}");
    var dbContext = services.GetRequiredService(type!);

    if (dbContext is DbContext db && db is TIDbContext interfaceDbContext)
    {
      return (db, interfaceDbContext);
    }

    throw new InvalidCastException($"Expected to find {typeof(DbContext)} but was actually {dbContext.GetType()}");
  }

  public IEnumerable<PropertyInfo> GetRequiredPropertiesForType(Type type)
  {
    var entityType = Db.Model.FindEntityType(type) ?? throw new InvalidDataException($"Could not find entity of type {type.Name} in DB Model");

    return entityType.GetProperties()
                      .Where(prop => !prop.IsNullable)
                      .Select(prop => prop.PropertyInfo)
                      .OfType<PropertyInfo>();
  }

  public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Db.SaveChangesAsync(cancellationToken);

  public IQueryable<TDbEntity> Include<TDbEntity, TProperty>(IQueryable<TDbEntity> queryable, Expression<Func<TDbEntity, TProperty>> expression) where TDbEntity : class
  => queryable.Include(expression);

  private static IEnumerable<Type> GetDbContexts() =>
    AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(type => type.IsAssignableTo(typeof(DbContext)) && !type.IsAbstract);

  private IEntityType GetEntityType<TDbEntity>() where TDbEntity : class
    => GetEntityType(typeof(TDbEntity));

  private IEntityType GetEntityType(Type type)
    => Db.Model.FindEntityType(type) ?? throw new KeyNotFoundException($"Could not find model in database for {type}");

  private DbSet<TDbEntity> GetDbSet<TDbEntity>() where TDbEntity : class => GetDbSet<TDbEntity>(GetEntityType<TDbEntity>());

  private DbSet<TDbEntity> GetDbSet<TDbEntity>(IEntityType model) where TDbEntity : class
  => (DbSet<TDbEntity>)Db.GetType().GetMethod("Set", 1, Type.EmptyTypes)!.MakeGenericMethod(model!.ClrType)!.Invoke(concreteDb, null)!;
}