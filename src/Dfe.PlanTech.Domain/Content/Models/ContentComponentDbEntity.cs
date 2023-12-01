using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

public abstract class ContentComponentDbEntity : IContentComponentDbEntity
{
  public string Id { get; set; } = null!;
}