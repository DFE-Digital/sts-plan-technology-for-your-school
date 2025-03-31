using Dfe.PlanTech.Domain.Groups;

namespace Dfe.PlanTech.Web.Models;

public class GroupsDashboardViewComponentViewModel
{
    public IList<GroupsCategorySectionDto> GroupsCategorySectionDto { get; init; } = null!;

    public string? NoSectionsErrorRedirectUrl { get; set; }
}
