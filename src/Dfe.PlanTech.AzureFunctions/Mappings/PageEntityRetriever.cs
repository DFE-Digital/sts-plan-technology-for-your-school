using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class PageEntityRetriever(CmsDbContext db) : EntityRetriever(db)
{
    public override async Task<ContentComponentDbEntity?> GetExistingDbEntity(ContentComponentDbEntity entity, CancellationToken cancellationToken)
    {
        var page = await Db.Pages.IgnoreQueryFilters()
                                .Include(page => page.AllPageContents)
                                .Include(page => page.BeforeTitleContent)
                                .Include(page => page.Content)
                                .Include(page => page.Title)
                                .FirstOrDefaultAsync(page => page.Id == entity.Id, cancellationToken);

        if (page == null)
        {
            return null;
        }

        return page;
    }
}