using Dfe.PlanTech.Domain.Content.Enums;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

/// <summary>
/// Model for Header content type from Contentful
/// </summary>
public interface IHeader
{
    /// <summary>
    /// The text to display
    /// </summary>
    public string? Text { get; }

    /// <summary>
    /// What tag should be used for header (e.g. H1, H2, etc.)
    /// </summary>
    public HeaderTag? Tag { get; }

    /// <summary>
    /// Actual size of header to use (for accessibility)
    /// </summary>
    public HeaderSize? Size { get; }
}
