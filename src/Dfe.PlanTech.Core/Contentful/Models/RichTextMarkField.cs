namespace Dfe.PlanTech.Core.Contentful.Models;

/// <summary>
/// Mark type for the rich text (e.g. bold, underline)
/// </summary>
/// <inheritdoc/>
public class RichTextMarkField : ContentfulEntry
{
    public string Type { get; set; } = "";
}
