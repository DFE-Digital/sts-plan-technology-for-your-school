using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class PageEntityUpdater(ILogger<PageEntityUpdater> logger, CmsDbContext db) : EntityUpdater(logger, db)
{
  public override MappedEntity UpdateEntityConcrete(MappedEntity entity)
  {
    if (!entity.AlreadyExistsInDatabase)
    {
      return entity;
    }

    if (entity.IncomingEntity is not PageDbEntity incomingPage || entity.ExistingEntity is not PageDbEntity existingPage)
    {
      throw new InvalidCastException($"Entities are not expected page types. Received {entity.IncomingEntity.GetType()} and {entity.ExistingEntity!.GetType()}");
    }

    var pageContentsJoined = incomingPage.AllPageContents.GroupJoin(existingPage.AllPageContents, KeySelector, KeySelector,
    (pageContent, inner) =>
    {
      return new
      {
        pageContent,
        existing = inner
      };
    });
  }

  private static readonly Func<PageContentDbEntity, CompositeKey> KeySelector = pageContent => new CompositeKey(pageContent.BeforeContentComponentId, pageContent.ContentComponentId);
}

public record CompositeKey(string? BeforeContentComponentId, string? ContentComponentId)
{
}