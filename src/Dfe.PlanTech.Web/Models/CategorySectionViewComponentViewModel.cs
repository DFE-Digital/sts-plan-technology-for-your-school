using Dfe.PlanTech.Domain.CategorySection;

namespace Dfe.PlanTech.Web.Models;

public class CategorySectionViewComponentViewModel
{
    public int CompletedSectionCount { get; init; }

    public int TotalSectionCount { get; init; }

    public IList<CategorySectionDto> CategorySectionDto { get; init; } = null!;

    public string? NoSectionsErrorRedirectUrl { get; set; }

    public string? ProgressRetrievalErrorMessage { get; init; }
}