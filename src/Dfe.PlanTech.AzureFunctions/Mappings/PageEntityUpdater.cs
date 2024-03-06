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

    AddOrUpdate(incomingPage, existingPage);
    RemoveOld(incomingPage, existingPage);

    return entity;
  }

  private static void RemoveOld(PageDbEntity incomingPage, PageDbEntity existingPage)
  {
    existingPage.AllPageContents.RemoveAll(pc => !incomingPage.AllPageContents.Exists(apc => apc.Matches(pc)));
  }

  private void AddOrUpdate(PageDbEntity incomingPage, PageDbEntity existingPage)
  {
    foreach (var pageContent in incomingPage.AllPageContents)
    {
      var matching = existingPage.AllPageContents.Where(pc => pc.Matches(pageContent)).ToList();

      if (matching.Count == 0)
      {
        existingPage.AllPageContents.Add(pageContent);
      }
      else
      {
        if (matching.Count > 1)
        {
          Db.RemoveRange(matching[1..]);
        }

        matching.First().Order = pageContent.Order;
      }
    }
  }
}
