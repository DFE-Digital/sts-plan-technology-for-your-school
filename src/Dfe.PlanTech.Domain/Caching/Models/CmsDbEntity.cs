namespace Dfe.PlanTech.Domain.Caching.Models;

public abstract class CmsDbEntity
{
    public long Id { get; private set; }

    public string ContentTypeId { get; init; } = null!;

    public string ContentId { get; init; } = null!;

    public bool IsPublished { get; set; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}