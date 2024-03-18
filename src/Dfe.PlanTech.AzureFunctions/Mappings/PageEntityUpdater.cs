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

  private static void RemoveOldPageContents(PageDbEntity incoming, PageDbEntity existing)
  {
    existing.AllPageContents.RemoveAll(pc => !incoming.AllPageContents.Exists(apc => apc.Matches(pc)));
  }

  private void AddOrUpdatePageContents(PageDbEntity incoming, PageDbEntity existing)
  {
    AddNewRelationshipsAndRemoveDuplicates<PageDbEntity, PageContentDbEntity, long>(incoming, existing, (page) => page.AllPageContents, UpdateOrder);
  }

  private static Action<PageContentDbEntity, PageContentDbEntity> UpdateOrder
  = (incoming, existing) =>
      {
        if (existing.Order != incoming.Order)
        {
          existing.Order = incoming.Order;
        }
      };
}
