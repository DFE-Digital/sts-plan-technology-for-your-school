using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class PageRetriever(IDatabaseHelper<ICmsDbContext> databaseHelper)
{
    public Task<PageDbEntity?> GetExistingDbEntity(PageDbEntity entity, CancellationToken cancellationToken)
      => databaseHelper.Database.FirstOrDefaultCachedAsync(databaseHelper.GetQueryableForEntityExcludingAutoIncludesAndFilters<PageDbEntity>()
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
}
