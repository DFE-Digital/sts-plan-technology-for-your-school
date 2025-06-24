using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Model for TextBody content type
/// </summary>
/// <inheritdoc/>
public class TextBody : ContentComponent, ITextBody<RichTextContent>
{
    public RichTextContent RichText { get; init; } = null!;
}
