using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport;

namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Data for a RichText section
/// </summary>
/// <inheritdoc/>
public class RichTextData : IRichTextData
{
    public string? Uri { get; init; }
}

public class CustomData : IRichTextData
{
    public string? Uri { get; init; }
    public Target Target { get; init; }
}
