using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Web.SeedTestData.ContentGenerators;

public abstract class ContentGenerator
{
    public abstract void CreateData();

    /// <summary>
    /// Helper method to generate a unique Id for a content component
    /// </summary>
    private static string GenerateId()
    {
        return Guid.NewGuid().ToString("n")[..20];
    }

    /// <summary>
    /// Helper method to create a ContentComponentDbEntity with Published = true
    /// </summary>
    protected static T CreateComponent<T>(T entity) where T : ContentComponentDbEntity
    {
        entity.Id = GenerateId();
        entity.Published = true;
        return entity;
    }

    /// <summary>
    /// Helper method to create a ContentComponentDbEntity with Published = false
    /// </summary>
    protected static T CreateDraftComponent<T>(T entity) where T : ContentComponentDbEntity
    {
        entity.Id = GenerateId();
        return entity;
    }
}
