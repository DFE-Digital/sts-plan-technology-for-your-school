using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Establishments.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Routing
{
    public class GroupsRouter : IGroupsRouter
    {
        private const string selectASchoolViewName = "~/Views/Groups/GroupsSelectSchool.cshtml";
        private const string schoolDashboardViewName = "~/Views/Groups/GroupsSchoolDashboard.cshtml";

        public IActionResult GetSchoolDashboard(string schoolId, string schoolName, string groupName, List<ContentComponent> content, GroupsController controller, CancellationToken cancellationToken)
        {
            var dashboardViewModel = new GroupsSchoolDashboardViewModel()
            {
                SchoolName = schoolName,
                SchoolId = schoolId,
                GroupName = groupName,
                Title = new Title() { Text = "Plan technology for your school" },
                Content = content,
                Slug = "dashboard"
            };
            return controller.View(schoolDashboardViewName, dashboardViewModel);
        }

        public IActionResult GetSelectASchool(string groupName, List<EstablishmentLink> schools, Title title, List<ContentComponent> content, GroupsController controller, CancellationToken cancellationToken)
        {
            var groupsViewModel = new GroupsSelectorViewModel()
            {
                GroupName = groupName,
                GroupEstablishments = schools,
                Title = title,
                Content = content
            };

            return controller.View(selectASchoolViewName, groupsViewModel);
        }
    }
}
