using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class PageEntityRetriever(CmsDbContext db) : EntityRetriever(db)
{
  public override async Task<ContentComponentDbEntity?> GetExistingDbEntity(ContentComponentDbEntity entity, CancellationToken cancellationToken)
  {
    var page = await Db.Pages.IgnoreQueryFilters()
                              .Select(page => new PageDbEntity()
                              {
                                AllPageContents = page.AllPageContents.Select(apc => new PageContentDbEntity()
                                {
                                  BeforeContentComponentId = apc.BeforeContentComponentId,
                                  ContentComponentId = apc.ContentComponentId,
                                  PageId = apc.PageId
                                }).ToList(),
                                Content = page.Content.Select(content => new ContentComponentDbEntity() { Id = content.Id }).ToList(),
                                BeforeTitleContent = page.Content.Select(content => new ContentComponentDbEntity() { Id = content.Id }).ToList(),
                                Archived = page.Archived,
                                Deleted = page.Deleted,
                                Published = page.Published,
                                InternalName = page.InternalName,
                                Slug = page.Slug,
                                DisplayBackButton = page.DisplayBackButton,
                                DisplayHomeButton = page.DisplayHomeButton,
                                DisplayOrganisationName = page.DisplayOrganisationName,
                                DisplayTopicTitle = page.DisplayTopicTitle,
                                RequiresAuthorisation = page.RequiresAuthorisation,
                                Id = page.Id,
                                TitleId = page.TitleId
                              })
                            .FirstOrDefaultAsync(page => page.Id == entity.Id, cancellationToken);

    if (page == null)
    {
      return null;
    }

    return page;
  }
}