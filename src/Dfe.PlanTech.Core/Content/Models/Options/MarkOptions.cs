namespace Dfe.PlanTech.Domain.Content.Models.Options;

/// <summary>
/// Class for mapping a "Mark" (e.g. bold, or underline, etc.) from Contentful API response, for Rich Text, to an appropriate tag
/// </summary>
public class MarkOption
{
    /// <summary>
    /// What mark this options is for (e.g. "bold" or "underline")
    /// </summary>
    /// <value></value>
    public required string Mark { get; init; }

    /// <summary>
    /// What HTML tag should be generated (e.g. "strong")
    /// </summary>
    /// <value></value>
    public required string HtmlTag { get; init; }

    /// <summary>
    /// Classes to add to generated tag
    /// </summary>
    /// <value></value>
    public string? Classes { get; init; }
}
