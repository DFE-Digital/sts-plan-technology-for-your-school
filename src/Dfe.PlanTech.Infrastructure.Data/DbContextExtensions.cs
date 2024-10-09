using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.Data;

public static class DbContextExtensions
{
  public static bool IsValidDbEntity<T>(this DbContext db, T entity, ILogger logger)
  {

    if (EqualityComparer<T>.Default.Equals(entity, default))
    {
      logger.LogWarning("Tried to attach null or default entity of type {EntityType}", typeof(T));
      return false;
    }

    var entityModel = db.Model.FindEntityType(entity!.GetType());

    if (entityModel == null)
    {
      logger.LogWarning("Entity type \"{EntityType}\" was not found in DbContext model", entity.GetType());
      return false;
    }

    var primaryKey = entityModel.FindPrimaryKey();

    if (primaryKey == null)
    {
      logger.LogWarning("Could not find primary key for type \"{EntityType}\" in IEntityType for type in DbContext", entity.GetType());
      return false;
    }

    foreach (var property in primaryKey.Properties)
    {
      if (property.PropertyInfo == null)
      {
        //Not sure if this can ever happen?
        continue;
      }

      var value = property.PropertyInfo.GetValue(entity);
      if (value == null)
      {
        //Cannot attach entity as PK is null
        return false;
      }
    }

    return true;
  }

  public static bool EntityAlreadyAttached<T>(this DbContext db, [DisallowNull] T entity) => db.ChangeTracker.Entries().Any(entry => entity.Equals(entry.Entity));
}