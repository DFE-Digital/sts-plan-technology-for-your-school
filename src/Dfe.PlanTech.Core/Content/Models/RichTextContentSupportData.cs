using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Core.Content.Models;

/// <summary>
/// Data for a RichTextContentSupportData section
/// </summary>
/// <inheritdoc/>
public class RichTextContentSupportData : IRichTextData
{
    public string? Uri { get; init; }
    public RichTextContentData? Target { get; init; }
}
