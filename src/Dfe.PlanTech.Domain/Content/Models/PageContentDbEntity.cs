using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dfe.PlanTech.Domain.Content.Models;

public class PageContentDbEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public string PageId { get; set; } = null!;
    public PageDbEntity Page { get; set; } = null!;

    public string? ContentComponentId { get; set; }
    public ContentComponentDbEntity? ContentComponent { get; set; }

    public string? BeforeContentComponentId { get; set; }
    public ContentComponentDbEntity? BeforeContentComponent { get; set; }

    /// <summary>
    /// What order the component should be in in its respective section (e.g. before/after)
    /// </summary>
    public int? Order { get; set; }

    public bool Matches(PageContentDbEntity other)
    {
        return other.PageId == PageId &&
                other.ContentComponentId == ContentComponentId &&
                other.BeforeContentComponentId == BeforeContentComponentId;
    }
}
