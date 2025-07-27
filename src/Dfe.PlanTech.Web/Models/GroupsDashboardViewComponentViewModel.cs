namespace Dfe.PlanTech.Web.Models;

public class GroupsDashboardViewComponentViewModel
{
    public ContentComponent Description { get; set; } = null!;

    public IList<GroupsCategorySectionDto> GroupsCategorySectionDto { get; init; } = null!;

    public string? NoSectionsErrorRedirectUrl { get; set; }

    public string? ProgressRetrievalErrorMessage { get; init; }
}
