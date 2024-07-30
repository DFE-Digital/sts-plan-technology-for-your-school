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
