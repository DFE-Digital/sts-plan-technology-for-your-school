using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class EntityRetriever(CmsDbContext db)
{
    public readonly CmsDbContext Db = db;

    /// <summary>
    /// Gets the existing entity (if existing) from the database that matches the mapped entity
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">Exception thrown when we do not have a matching DbSet in our DbContext for the entity type</exception>
    public virtual Task<ContentComponentDbEntity?> GetExistingDbEntity(ContentComponentDbEntity entity, CancellationToken cancellationToken)
    {
        var model = Db.Model.FindEntityType(entity.GetType()) ?? throw new KeyNotFoundException($"Could not find model in database for {entity.GetType()}");

        var dbSet = GetIQueryableForEntity(model);

        return dbSet.IgnoreAutoIncludes()
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(existing => existing.Id == entity.Id, cancellationToken);
    }

    /// <summary>
    /// Uses reflection to get the DbSet, as an IQueryable, for the provided entity 
    /// </summary>
    /// <param name="model">Entity type for the entity we have received</param>
    /// <returns></returns>
    private IQueryable<ContentComponentDbEntity> GetIQueryableForEntity(IEntityType model)
    => (IQueryable<ContentComponentDbEntity>)Db.GetType()
                                              .GetMethod("Set", 1, Type.EmptyTypes)!
                                              .MakeGenericMethod(model!.ClrType)!
                                              .Invoke(Db, null)!;
}
