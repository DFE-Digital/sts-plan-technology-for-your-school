using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models.Buttons;

/// <summary>
/// Class for the table for the <see cref="ButtonWithLink"/> button that links somewhere 
/// </summary>
/// <inheritdoc/>
public class ButtonWithLinkDbEntity : ContentComponentDbEntity, IButtonWithLink<ButtonDbEntity>
{
    [DontCopyValue]
    public ButtonDbEntity? Button { get; set; } = null!;

    public string ButtonId { get; set; } = null!;

    public string Href { get; set; } = null!;
}
