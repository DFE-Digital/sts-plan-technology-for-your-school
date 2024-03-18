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

    var (incoming, existing) = MapToConcreteType<PageDbEntity>(entity);

    AddOrUpdatePageContents(incoming, existing);
    RemoveOldPageContents(incoming, existing);

    return entity;
  }

  private void RemoveOldPageContents(PageDbEntity incomingPage, PageDbEntity existingPage)
  {
    var removedPageContents = existingPage.AllPageContents.Where(pc => !incomingPage.AllPageContents.Exists(apc => apc.Matches(pc)));
    Db.PageContents.RemoveRange(removedPageContents);
  }

  private void AddOrUpdatePageContents(PageDbEntity incomingPage, PageDbEntity existingPage)
  {
    foreach (var pageContent in incomingPage.AllPageContents)
    {
      var matchingContents = existingPage.AllPageContents.Where(pc => pc.Matches(pageContent))
                                                        .OrderByDescending(pc => pc.Id)
                                                        .ToList();

      if (matchingContents.Count == 0)
      {
        existingPage.AllPageContents.Add(pageContent);
        continue;
      }

      if (matchingContents.Count > 1)
      {
        Db.PageContents.RemoveRange(matchingContents[1..]);
      }

      var remainingMatchingContent = matchingContents[0];

      if (remainingMatchingContent.Order != pageContent.Order)
      {
        remainingMatchingContent.Order = pageContent.Order;
      }
    }
  }
}
