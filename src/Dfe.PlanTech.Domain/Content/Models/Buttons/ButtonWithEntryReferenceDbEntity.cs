using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models.Buttons;

/// <summary>
/// A button that links to a different entry
/// </summary>
public class ButtonWithEntryReferenceDbEntity : ContentComponentDbEntity, IButtonWithEntryReference<ButtonDbEntity, ContentComponentDbEntity>
{
    public string InternalName { get; set; } = null!;

    [DontCopyValue]
    public ButtonDbEntity Button { get; set; } = null!;

    public string ButtonId { get; set; } = null!;

    [DontCopyValue]
    public ContentComponentDbEntity LinkToEntry { get; set; } = null!;

    public string LinkToEntryId { get; set; } = null!;

    [DontCopyValue]
    public string Slug { get; set; } = "";

    [DontCopyValue]
    public LinkToEntryType LinkType { get; set; } = LinkToEntryType.Unknown;
}

