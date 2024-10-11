namespace Dfe.PlanTech.Domain.Content.Models.Buttons;

public class ButtonWithEntryReferenceLinkDbEntity
{
  public ButtonWithEntryReferenceDbEntity ButtonWithEntryReference { get; set; } = null!;
  public string ButtonWithEntryReferenceId { get; set; } = null!;
  public string Slug { get; set; } = "";
  public string LinkType { get; set; } = "";
}