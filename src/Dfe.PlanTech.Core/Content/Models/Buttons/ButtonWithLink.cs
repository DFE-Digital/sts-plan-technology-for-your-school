using Dfe.PlanTech.Core.Content.Models;
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models.Buttons;

/// <summary>
/// A button that links somewhere
/// </summary>
public class ButtonWithLink : ContentComponent, IButtonWithLink<ComponentButtonEntry>
{
    public ComponentButtonEntry Button { get; init; } = null!;

    public string Href { get; init; } = null!;
}
