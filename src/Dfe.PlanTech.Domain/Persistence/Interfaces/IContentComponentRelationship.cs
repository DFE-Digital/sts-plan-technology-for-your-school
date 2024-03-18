using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Persistence.Interfaces;

public interface IContentComponentRelationship
{
  public ContentComponentDbEntity? ContentComponent { get; set; }
}