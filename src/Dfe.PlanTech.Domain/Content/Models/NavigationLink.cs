using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

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
    public string? DisplayText { get; set; }

    /// <summary>
    /// Href value (i.e. <a href="{Href}"></a>)
    /// </summary>
    public string? Href { get; set; }

    /// <summary>
    /// Should this link open in a new tab?
    /// </summary>
    public bool OpenInNewTab { get; set; } = false;
}
