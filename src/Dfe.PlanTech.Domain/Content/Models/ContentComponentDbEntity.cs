using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

public abstract class ContentComponentDbEntity : IContentComponentDbEntity
{
    public string Id { get; set; } = null!;

    public bool Published { get; set; }

    public bool Archived { get; set; }

    public bool Deleted { get; set; }

    [DontCopyValue]
    public long? Order { get; set; }

    public List<PageDbEntity> BeforeTitleContentPages { get; set; } = new();

    public List<PageDbEntity> ContentPages { get; set; } = new();
}