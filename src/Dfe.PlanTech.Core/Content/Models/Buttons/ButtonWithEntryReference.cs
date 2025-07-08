using Dfe.PlanTech.Core.Content.Models;
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models.Buttons;

/// <summary>
/// A button that links to a different entry
/// </summary>
public class ButtonWithEntryReference : ContentComponent, IButtonWithEntryReference<ComponentButtonEntry, ContentComponent>
{
    public ComponentButtonEntry Button { get; init; } = null!;

    public ContentComponent LinkToEntry { get; init; } = null!;
}
