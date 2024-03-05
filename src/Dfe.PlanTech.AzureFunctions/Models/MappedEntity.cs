using System.Reflection;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Models;

public class MappedEntity
{
  public required ContentComponentDbEntity IncomingEntity { get; init; }

  public ContentComponentDbEntity? ExistingEntity { get; init; }

  public bool IsValid { get; set; }

  public bool AlreadyExistsInDatabase => ExistingEntity != null;

  public bool IsValidComponent(CmsDbContext db, Type dontCopyAttribute, ILogger logger)
  {
    string? nullProperties = string.Join(", ", AnyRequiredPropertyIsNull(db, dontCopyAttribute));

    IsValid = !string.IsNullOrEmpty(nullProperties);

    if (!IsValid)
    {
      logger.LogInformation("Content Component with ID {id} is missing the following required properties: {nullProperties}", IncomingEntity.Id, nullProperties);
    }

    return IsValid;
  }

  private IEnumerable<PropertyInfo?> AnyRequiredPropertyIsNull(CmsDbContext db, Type dontCopyAttribute)
      => db.Model.FindEntityType(IncomingEntity.GetType())!
      .GetProperties()
      .Where(prop => !prop.IsNullable)
      .Select(prop => prop.PropertyInfo)
      .Where(prop => !prop!.CustomAttributes.Any(atr => atr.GetType() == dontCopyAttribute))
      .Where(prop => prop!.GetValue(IncomingEntity) == null);
}