namespace Dfe.PlanTech.Core.Contentful.Interfaces;

/// <summary>
/// Data for a RichText section
/// </summary>
/// <inheritdoc/>
public interface IHasUri
{
    /// <summary>
    /// URL for a link
    /// </summary>
    public string? Uri { get; init; }
}
