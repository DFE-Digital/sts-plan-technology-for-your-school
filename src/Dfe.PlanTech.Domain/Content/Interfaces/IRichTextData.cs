using Contentful.Core.Models;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

/// <summary>
/// Data for a RichText section
/// </summary>
/// <inheritdoc/>
public interface IRichTextData
{
    /// <summary>
    /// URL for a link
    /// </summary>
    public string? Uri { get; init; }
}

public interface ICardComponentData
{
    public string? Title { get; }
    public string? Description { get; }
    public string? Meta { get; }
    public Asset? Image { get; }
    public string? ImageAlt { get; }
    public string? Uri { get; }
}
