using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Web.ViewModels;

public class GroupsDashboardViewComponentViewModel
{
    public CmsEntryDto Description { get; set; } = null!;

    public List<GroupsCategorySectionViewModel> GroupsCategorySection { get; init; } = null!;

    public string? NoSectionsErrorRedirectUrl { get; set; }

    public string? ProgressRetrievalErrorMessage { get; init; }
}
