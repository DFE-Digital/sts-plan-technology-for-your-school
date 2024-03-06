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

  private void RemoveOld(PageDbEntity incomingPage, PageDbEntity existingPage)
  {
    var removedPageContents = existingPage.AllPageContents.Where(pc => !incomingPage.AllPageContents.Exists(apc => apc.Matches(pc)));
    Db.PageContents.RemoveRange(removedPageContents);
  }

  private void AddOrUpdate(PageDbEntity incomingPage, PageDbEntity existingPage)
  {
    foreach (var pageContent in incomingPage.AllPageContents)
    {
      var matchingContents = existingPage.AllPageContents.Where(pc => pc.Matches(pageContent))
                                                  .OrderByDescending(pc => pc.Id)
                                                  .ToList();

      if (matchingContents.Count == 0)
      {
        existingPage.AllPageContents.Add(pageContent);
      }
      else
      {
        if (matchingContents.Count > 1)
        {
          Db.PageContents.RemoveRange(matchingContents[1..]);
        }

        var remainingMatchingContent = matchingContents.First();
        if (remainingMatchingContent.Order != pageContent.Order)
        {
          remainingMatchingContent.Order = pageContent.Order;
        }
      }
    }
  }
}
