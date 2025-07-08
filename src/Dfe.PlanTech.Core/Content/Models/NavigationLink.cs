using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Core.Content.Models;

/// <summary>
/// A navigation link
/// </summary>
/// <remarks>
/// Currently only used in footer. Could be extended to be both in future
/// </remarks>
public class NavigationLink : ContentComponent, INavigationLink
{
    /// <summary>
    /// Display text (i.e. <a>{DisplayText}</a>)
    /// </summary>
    public string DisplayText { get; set; } = null!;

    /// <summary>
    /// Href value (i.e. <a href="{Href}"></a>)
    /// </summary>
    public string? Href { get; set; } = null;

    /// <summary>
    /// Should this link open in a new tab?
    /// </summary>
    public bool OpenInNewTab { get; set; } = false;

    /// <summary>
    /// The content to link to.
    /// </summary>
    public IContentComponent? ContentToLinkTo { get; set; }
}
