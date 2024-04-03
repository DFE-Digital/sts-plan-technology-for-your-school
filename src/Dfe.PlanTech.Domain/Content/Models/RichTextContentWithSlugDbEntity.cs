namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Represents a rich text content entity with the corresponding slug for the page that it is loaded on
/// </summary>
/// <remarks>
/// Is loaded by a view
/// </remarks>
public class RichTextContentWithSlugDbEntity : RichTextContentDbEntity
{
    //Loaded from page that the parent entity (e.g. TextBodyDbEntity) belongs to
    public string Slug { get; set; } = null!;

    //Loaded from the parent entity
    public bool Archived { get; set; }
    public bool Deleted { get; set; }
    public bool Published { get; set; }
}
