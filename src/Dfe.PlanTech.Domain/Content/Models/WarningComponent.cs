
namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Model for Warning Component content type
/// </summary>
public class WarningComponent : ContentComponent
{
  public TextBody Text { get; init; } = null!;
}