using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Persistence.Interfaces;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class PageEntityRetriever(IDatabaseHelper<ICmsDbContext> databaseHelper)
{
    public async Task<PageDbEntity?> GetExistingDbEntity(PageDbEntity entity, CancellationToken cancellationToken)
    {
        var page = await databaseHelper.Database.FirstOrDefaultAsync(databaseHelper.GetQueryableForEntityExcludingAutoIncludesAndFilters<PageDbEntity>()
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
                                    Published = page.Published,
                                    Archived = page.Archived,
                                    Deleted = page.Deleted
                                })
                                .Where(page => page.Id == entity.Id), cancellationToken);

        if (page == null)
        {
            return null;
        }

        return page;
    }
}
