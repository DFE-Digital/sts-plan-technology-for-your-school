using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dfe.PlanTech.Domain.Content.Models;

public class PageContentDbEntity
{
    public string PageId { get; set; } = null!;
    public PageDbEntity Page { get; set; } = null!;
    public string? ContentComponentId { get; set; }
    public ContentComponentDbEntity? ContentComponent { get; set; }

    public string? BeforeContentComponentId { get; set; }
    public ContentComponentDbEntity? BeforeContentComponent { get; set; }
}
