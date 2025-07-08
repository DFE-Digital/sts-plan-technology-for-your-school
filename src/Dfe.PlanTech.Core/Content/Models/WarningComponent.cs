
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Core.Content.Models;

public class WarningComponent : ContentComponent, IWarningComponent<TextBody>
{
    public TextBody Text { get; init; } = null!;
}
