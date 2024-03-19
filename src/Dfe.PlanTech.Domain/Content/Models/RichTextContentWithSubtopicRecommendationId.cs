namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Represents a rich text content entity with the corresponding slug for the page that it is loaded on
/// </summary>
/// <remarks>
/// Is loaded by a view
/// </remarks>
public class RichTextContentWithSubtopicRecommendationId : RichTextContentDbEntity
{
    //Loaded from page that the parent entity (e.g. TextBodyDbEntity) belongs to
    public string SubtopicRecommendationId { get; set; } = null!;
}
