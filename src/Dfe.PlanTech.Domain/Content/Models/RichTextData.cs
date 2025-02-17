using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Data for a RichText section
/// </summary>
/// <inheritdoc/>
public class RichTextData : IRichTextData
{
    public string? Uri { get; init; }
}
