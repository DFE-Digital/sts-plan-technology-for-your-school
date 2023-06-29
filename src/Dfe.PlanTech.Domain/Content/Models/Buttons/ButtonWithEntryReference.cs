using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models.Buttons;

/// <summary>
/// A button that links to a different entry
/// </summary>
public class ButtonWithEntryReference : ContentComponent
{
    public Button Button { get; init; } = null!;

    public IContentComponent LinkToEntry { get; init; } = null!;
}
