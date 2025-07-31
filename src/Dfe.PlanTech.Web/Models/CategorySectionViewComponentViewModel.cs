using Dfe.PlanTech.Domain.CategorySection;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Web.Models;

public class CategorySectionViewComponentViewModel
{
    public ContentComponent Description { get; set; } = null!;

    public string CategoryHeaderText { get; set; } = null!;

    public int CompletedSectionCount { get; init; }

    public int TotalSectionCount { get; init; }

    public IList<CategorySectionDto> CategorySectionDto { get; init; } = null!;

    public string? CategorySlug { get; set; }

    public string? NoSectionsErrorRedirectUrl { get; set; }

    public string? ProgressRetrievalErrorMessage { get; init; }
}
