using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dfe.PlanTech.Domain.Content.Models;

public class PageContentDbEntity
{
    public string PageId { get; set; } = null!;
    public PageDbEntity Page { get; set; } = null!;

    public string ContentComponentId { get; set; } = null!;
    public ContentComponentDbEntity ContentComponent { get; set; } = null!;
}
