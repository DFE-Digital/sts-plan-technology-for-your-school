using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.Data;

public static class DbContextExtensions
{
  /// <summary>
  /// Validates that "entity" is an object that is valid to "blindly" attach to the DB
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="db"></param>
  /// <param name="entity"></param>
  /// <param name="logger"></param>
  /// <returns></returns>
  public static bool IsValidDbEntity<T>(this DbContext db, T entity, ILogger logger)
  {
    if (!EntityHasValue(entity, logger))
      return false;

    var entityModel = db.Model.FindEntityType(entity!.GetType());

    if (entityModel == null)
    {
      logger.LogWarning("Entity type \"{EntityType}\" was not found in DbContext model", entity.GetType());
      return false;
    }

    return HaveValidPrimaryKey(entity, logger, entityModel);
  }

  private static bool HaveValidPrimaryKey<T>(T entity, ILogger logger, IEntityType entityModel)
  {
    var primaryKey = entityModel.FindPrimaryKey();

    if (primaryKey == null)
    {
      logger.LogWarning("Could not find primary key for type \"{EntityType}\" in IEntityType for type in DbContext", entity!.GetType());
      return false;
    }

    bool hasNullPrimaryKeyValue = primaryKey.Properties
        .Where(property => property.PropertyInfo != null)
        .Select(property => property.PropertyInfo?.GetValue(entity))
        .Any(value => value == null);

    return !hasNullPrimaryKeyValue;
  }

  private static bool EntityHasValue<T>(T entity, ILogger logger)
  {
    if (!EqualityComparer<T>.Default.Equals(entity, default))
      return true;

    logger.LogWarning("Tried to attach null or default entity of type {EntityType}", typeof(T));
    return false;
  }

  public static bool EntityAlreadyAttached<T>(this DbContext db, [DisallowNull] T entity)
  => db.ChangeTracker.Entries().Any(entry => EntitiesMatch(entity, entry.Entity));

  private static bool EntitiesMatch(object? x, object? y)
  {
    if (ReferenceEquals(x, y))
      return true;

    if (x is null || y is null)
      return false;

    if (x is ContentComponentDbEntity firstContentComponent && y is ContentComponentDbEntity secondContentComponent)
    {
      return firstContentComponent.Id == secondContentComponent.Id;
    }

    return false;
  }
}
