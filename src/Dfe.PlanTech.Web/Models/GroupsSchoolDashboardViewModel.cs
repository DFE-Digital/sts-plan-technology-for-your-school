using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Web.Models
{
    public class GroupsSchoolDashboardViewModel
    {
        public string Slug { get; set; } = null!;

        public string? SchoolName { get; set; }

        public string SchoolId { get; set; } = null!;

        public string GroupName { get; set; } = null!;

        public Title Title { get; set; } = null!;

        public List<ContentComponent> Content { get; init; } = null!;

        public string? ErrorMessage { get; set; }
    }
}
