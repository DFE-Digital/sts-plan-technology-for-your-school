using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Web.SeedTestData.ContentGenerators;

public abstract class ContentGenerator
{
    public abstract void CreateData();

    private static string GenerateId()
    {
        return Guid.NewGuid().ToString("n")[..20];
    }

    /// <summary>
    /// Helper method to create a ContentComponentDbEntity with Published = true
    /// and a randomly generated Id
    /// </summary>
    protected static T CreateComponent<T>(T entity) where T: ContentComponentDbEntity
    {
        entity.Id = GenerateId();
        entity.Published = true;
        return entity;
    }
}
