namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// A navigation link
/// </summary>
/// <remarks>
/// Currently only used in footer. Could be extended to be both in future
/// </remarks>
public class NavigationLink : ContentComponent
{
  /// <summary>
  /// Display text (i.e. <a>{DisplayText}</a>)
  /// </summary>
  public string DisplayText { get; init; } = null!;

  /// <summary>
  /// Href value (i.e. <a href="{Href}"></a>)
  /// </summary>
  public string Href { get; init; } = null!;
}