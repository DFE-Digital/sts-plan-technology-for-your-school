using System.Linq.Expressions;
using System.Reflection;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.PlanTech.Infrastructure.Data;

/// <inheritdoc cref="IDatabaseHelper{TIDbContext}"/>
public class DatabaseHelper<TIDbContext> : IDatabaseHelper<TIDbContext>
where TIDbContext : IDbContext
{
    private readonly DbContext _concreteDb;
    private readonly TIDbContext _abstractDb;

    public TIDbContext Database => _abstractDb;

    public DatabaseHelper(IServiceProvider services)
    {
        var (db, interfaceDb) = GetDbContext(services);
        _concreteDb = db;
        _abstractDb = interfaceDb;
    }

    public void ClearTracking()
    {
        _concreteDb.ChangeTracker.Clear();
    }

    public IQueryable<TDbEntity> GetIQueryableForEntity<TDbEntity>() where TDbEntity : class
    => GetDbSet<TDbEntity>(GetEntityType<TDbEntity>());

    public IQueryable<TDbEntity> GetQueryableForEntityExcludingAutoIncludesAndFilters<TDbEntity>()
        where TDbEntity : class
        => GetDbSet<TDbEntity>().IgnoreAutoIncludes().IgnoreQueryFilters();

    public IQueryable<TDbEntity> GetQueryableForEntityExcludingAutoIncludesAndFilters<TDbEntity>(TDbEntity entity)
        where TDbEntity : class
        => GetDbSet<TDbEntity>().IgnoreAutoIncludes().IgnoreQueryFilters();

    public void Add<TDbEntity>(TDbEntity entity) where TDbEntity : class => _concreteDb.Add(entity);

    public void Update<TDbEntity>(TDbEntity entity) where TDbEntity : class => _concreteDb.Update(entity);

    public void Remove<TDbEntity>(TDbEntity entity) where TDbEntity : class => _concreteDb.Remove(entity);

    /// <summary>
    /// Gets properties of the type that are non-nullable in the Db
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="InvalidDataException"></exception>
    public IEnumerable<PropertyInfo> GetRequiredPropertiesForType(Type type)
    {
        var entityType = _concreteDb.Model.FindEntityType(type) ?? throw new InvalidDataException($"Could not find entity of type {type.Name} in DB Model");

        return entityType.GetProperties()
                          .Where(prop => !prop.IsNullable)
                          .Select(prop => prop.PropertyInfo)
                          .OfType<PropertyInfo>();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => _concreteDb.SaveChangesAsync(cancellationToken);

    public IQueryable<TDbEntity> Include<TDbEntity, TProperty>(IQueryable<TDbEntity> queryable, Expression<Func<TDbEntity, TProperty>> expression) where TDbEntity : class
    => queryable.Include(expression);

    private static IEnumerable<Type> GetDbContexts() =>
      AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(type => type.IsAssignableTo(typeof(DbContext)) && !type.IsAbstract);

    private IEntityType GetEntityType<TDbEntity>() where TDbEntity : class
      => GetEntityType(typeof(TDbEntity));

    private IEntityType GetEntityType(Type type)
      => _concreteDb.Model.FindEntityType(type) ?? throw new KeyNotFoundException($"Could not find model in database for {type}");

    private DbSet<TDbEntity> GetDbSet<TDbEntity>() where TDbEntity : class
        => GetDbSet<TDbEntity>(GetEntityType<TDbEntity>());

    private DbSet<TDbEntity> GetDbSet<TDbEntity>(IEntityType model) where TDbEntity : class
        => GetDbSetObject<DbSet<TDbEntity>>(model);

    private TOut GetDbSetObject<TOut>(IEntityType model)
        => (TOut)_concreteDb.GetType()
                      .GetMethod("Set", 1, Type.EmptyTypes)!
                      .MakeGenericMethod(model!.ClrType)!
                      .Invoke(_concreteDb, null)!;

    /// <summary>
    /// Gets the matching DbContext for the TIDbContext interface
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="InvalidCastException"></exception>
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
}
