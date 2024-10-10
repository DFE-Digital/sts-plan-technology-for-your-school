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
    /// Helper method to set Internal name to empty string for any component that has it
    /// </summary>
    private static void SetInternalNameDefault<T>(T entity) where T : ContentComponentDbEntity
    {
        var internalName = entity.GetType().GetProperty("InternalName");
        if (internalName != null && internalName.GetValue(entity) == null) internalName.SetValue(entity, "");
    }

    /// <summary>
    /// Helper method to create a ContentComponentDbEntity with Published = true
    /// </summary>
    protected static T CreateComponent<T>(T entity) where T : ContentComponentDbEntity
    {
        entity.Id = GenerateId();
        entity.Published = true;
        SetInternalNameDefault(entity);
        return entity;
    }

    /// <summary>
    /// Helper method to create a ContentComponentDbEntity with Published = false
    /// </summary>
    protected static T CreateDraftComponent<T>(T entity) where T : ContentComponentDbEntity
    {
        entity.Id = GenerateId();
        SetInternalNameDefault(entity);
        return entity;
    }

    /// <summary>
    /// Helper method for a text body with rich text contents
    /// </summary>
    protected static TextBodyDbEntity CreateTextBody(string text, bool published = true)
    {
        var textBody = CreateComponent(new TextBodyDbEntity()
        {
            RichText = new RichTextContentDbEntity()
            {
                Data = new RichTextDataDbEntity() { Uri = "uri" },
                Marks = [new RichTextMarkDbEntity() { Type = "Bold" }],
                Value = text,
                NodeType = "paragraph"
            }
        });
        if (!published)
            textBody.Published = false;
        return textBody;
    }
}
