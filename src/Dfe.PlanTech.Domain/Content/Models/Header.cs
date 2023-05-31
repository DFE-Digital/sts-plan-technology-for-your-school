using Dfe.PlanTech.Domain.Content.Enums;

namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Model for Header content type
/// </summary>
public class Header : ContentComponent
{
    /// <summary>
    /// The text to display
    /// </summary>
    public string Text { get; init; } = null!;

    /// <summary>
    /// What tag should be used for header (e.g. H1, H2, etc.)
    /// </summary>
    public HeaderTag Tag { get; init; }

    /// <summary>
    /// Actual size of header to use (for accessibility)
    /// </summary>
    public HeaderSize Size { get; init; }
}
