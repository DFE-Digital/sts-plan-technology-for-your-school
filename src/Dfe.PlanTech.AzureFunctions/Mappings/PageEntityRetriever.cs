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
                                    Id = page.Id,
                                    Slug = page.Slug,
                                    TitleId = page.TitleId,
                                    DisplayBackButton = page.DisplayBackButton,
                                    DisplayHomeButton = page.DisplayHomeButton,
                                    DisplayOrganisationName = page.DisplayOrganisationName,
                                    DisplayTopicTitle = page.DisplayTopicTitle,
                                    RequiresAuthorisation = page.RequiresAuthorisation,
                                    AllPageContents = page.AllPageContents.Select(pageContent => new PageContentDbEntity()
                                    {
                                        BeforeContentComponentId = pageContent.BeforeContentComponentId,
                                        ContentComponentId = pageContent.ContentComponentId,
                                        Id = pageContent.Id,
                                        PageId = pageContent.PageId,
                                        Order = pageContent.Order ?? 0,
                                    }).ToList(),
                                })
                                .FirstOrDefaultAsync(page => page.Id == entity.Id, cancellationToken);

        if (page == null)
        {
            return null;
        }

        return page;
    }
}