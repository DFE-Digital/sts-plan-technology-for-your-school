using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models.Buttons;

/// <summary>
/// A button that links to a different entry
/// </summary>
public class ButtonWithEntryReferenceDbEntity : ContentComponentDbEntity, IButtonWithEntryReference<ButtonDbEntity, ContentComponentDbEntity>
{
    [DontCopyValue]
    public ButtonDbEntity Button { get; set; } = null!;

    public string ButtonId { get; set; } = null!;

    [DontCopyValue]
    public ContentComponentDbEntity LinkToEntry { get; set; } = null!;

    public string LinkToEntryId { get; set; } = null!;

    [DontCopyValue]
    public ButtonWithEntryReferenceLinkDbEntity? Link { get; set; } = new();

    public LinkToEntryType LinkType => Enum.TryParse<LinkToEntryType>(Link?.LinkType, out var result) ? result : LinkToEntryType.Unknown;
}
