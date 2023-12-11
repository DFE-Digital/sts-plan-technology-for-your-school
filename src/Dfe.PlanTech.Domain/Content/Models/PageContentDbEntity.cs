namespace Dfe.PlanTech.Domain.Content.Models;

public class PageContentDbEntity
{
    public PageDbEntity Page { get; set; } = null!;

    public ContentComponentDbEntity ContentComponent { get; set; } = null!;
}
