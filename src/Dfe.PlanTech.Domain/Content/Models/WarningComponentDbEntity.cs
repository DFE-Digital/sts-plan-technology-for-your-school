
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

public class WarningComponentDbEntity : ContentComponentDbEntity, IWarningComponent<TextBodyDbEntity>
{
    public string TextId { get; set; } = null!;
    public TextBodyDbEntity Text { get; set; } = null!;
}