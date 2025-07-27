using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Web.Models;

public class CategorySectionViewComponentViewModel
{
    public IList<CategorySectionViewModel> CategorySections { get; init; } = null!;
    public int CompletedSectionCount { get; init; }
    public CmsEntryDto Description { get; set; } = null!;
    public string? NoSectionsErrorRedirectUrl { get; set; }
    public string? ProgressRetrievalErrorMessage { get; init; }
    public int TotalSectionCount { get; init; }
}
