using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models.Buttons;

/// <summary>
/// A button that links to a different entry
/// </summary>
public class ButtonWithEntryReference : ContentComponent, IButtonWithEntryReference<Button, ContentComponent>
{
    public Button Button { get; init; } = null!;

    public ContentComponent LinkToEntry { get; init; } = null!;

    public ButtonWithEntryReferenceLink? Link { get; set; }

    public LinkToEntryType LinkType => Enum.TryParse<LinkToEntryType>(Link?.LinkType, out var result) ? result : LinkToEntryType.Unknown;
}
