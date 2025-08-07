using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Web.ViewModels;

public class GroupsSchoolDashboardViewModel
{
    public List<CmsEntryDto> Content { get; init; } = null!;
    public string? ErrorMessage { get; set; }
    public string GroupName { get; set; } = null!;
    public int SchoolId { get; set; }
    public string? SchoolName { get; set; }
    public string Slug { get; set; } = null!;
    public CmsComponentTitleDto Title { get; set; } = null!;
}
