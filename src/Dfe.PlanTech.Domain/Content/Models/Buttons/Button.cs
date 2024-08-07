using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models.Buttons;

public class Button : ContentComponent, IButton
{
    public string? Value { get; init; }

    public bool IsStartButton { get; init; }
}
