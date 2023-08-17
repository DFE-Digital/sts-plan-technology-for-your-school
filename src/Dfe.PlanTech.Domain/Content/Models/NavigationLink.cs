namespace Dfe.PlanTech.Domain.Content.Models;

public class NavigationLink : ContentComponent
{
  public string DisplayText { get; init; } = null!;

  public string Href { get; init; } = null!;
}