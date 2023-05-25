
namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Model for TextBody content type
/// </summary>
public class TextBody : ContentComponent
{
    public RichTextContent RichText { get; init; } = null!;
}