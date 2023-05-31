namespace Dfe.PlanTech.Domain.Content.Models.Buttons;

public class Button : ContentComponent
{
    public string Value { get; init; } = null!;

    public bool IsStartButton { get; init; }
}
