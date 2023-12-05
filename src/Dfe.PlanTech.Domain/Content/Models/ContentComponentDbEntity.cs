using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

public abstract class ContentComponentDbEntity : IContentComponentDbEntity
{
  public string Id { get; set; } = null!;

  public bool Published { get; set; }

  public bool Archived { get; set; }

  public bool Deleted { get; set; }

  public PageDbEntity[] BeforeTitleContentPages { get; set; } = Array.Empty<PageDbEntity>();

  public PageDbEntity[] ContentPages { get; set; } = Array.Empty<PageDbEntity>();
}