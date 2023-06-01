namespace Dfe.PlanTech.Domain.Content.Models.Buttons;

/// <summary>
/// A button that links somewhere
/// </summary>
public class ButtonWithLink : ContentComponent
{
    public Button Button { get; init; } = null!;

    public string Href { get; init; } = null!;
}
