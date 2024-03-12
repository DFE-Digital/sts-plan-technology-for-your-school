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

        AddOrUpdatePageContents(incomingPage, existingPage);
        RemoveOldPageContents(incomingPage, existingPage);

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
