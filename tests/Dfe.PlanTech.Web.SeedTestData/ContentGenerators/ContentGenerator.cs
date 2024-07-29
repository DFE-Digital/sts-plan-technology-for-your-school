using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.Web.SeedTestData.ContentGenerators;

public abstract class ContentGenerator(CmsDbContext db)
{
    public abstract void CreateData();

    /// <summary>
    /// Find a piece of content that has already been added
    /// </summary>
    protected ContentComponentDbEntity FindContentById(string id)
    {
        return db.ContentComponents.First(component => component.Id == id);
    }
}