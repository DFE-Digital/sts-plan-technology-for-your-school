using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Middleware;
using Dfe.PlanTech.Web.Routing;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers
{
    public class GroupsController : BaseController<GroupsController>
    {
        public const string GroupsSelectorPageSlug = "select-a-school";
        public const string GroupsSelectorViewName = "GroupsSelectSchool";
        public const string GroupsSchoolDashboardSlug = "dashboard";
        public const string GetSchoolDashboardAction = "GetSchoolDashboard";

        private readonly IExceptionHandlerMiddleware _exceptionHandler;
        private readonly IUser _user;
        private readonly ILogger _logger;
        private readonly IGroupsRouter _groupsRouter;

        public GroupsController(ILogger<GroupsController> logger, IExceptionHandlerMiddleware exceptionHandler, IUser user, IGroupsRouter groupsRouter) : base(logger)
        {
            _exceptionHandler = exceptionHandler;
            _user = user;
            _logger = logger;
            _groupsRouter = groupsRouter;
        }

        [HttpGet("groups/select-a-school")]
        public async Task<IActionResult> GetSelectASchoolView([FromServices] IGetPageQuery getPageQuery, CancellationToken cancellationToken)
        {
            var selectASchoolPageContent = await getPageQuery.GetPageBySlug(GroupsSelectorPageSlug, cancellationToken);

            var schools = await _user.GetGroupEstablishments();
            var groupName = _user.GetOrganisationData().OrgName;
            var title = new Title() { Text = groupName };
            List<ContentComponent> content = selectASchoolPageContent?.Content ?? new List<ContentComponent>();

            return _groupsRouter.GetSelectASchool(groupName, schools, title, content, this, cancellationToken);
        }

        [HttpPost("groups/select-a-school")]
        public IActionResult SelectSchool(string schoolId, string schoolName)
        {
            TempData["SelectedSchoolId"] = schoolId;
            TempData["SelectedSchoolName"] = schoolName;

            return RedirectToAction("GetSchoolDashboardView");
        }

        [HttpGet("groups/dashboard", Name = GetSchoolDashboardAction)]
        public async Task<IActionResult> GetSchoolDashboardView([FromServices] IGetPageQuery getPageQuery, CancellationToken cancellationToken)
        {
            var schoolId = TempData["SelectedSchoolId"] as string;
            var schoolName = TempData["SelectedSchoolName"] as string;
            var groupName = _user.GetOrganisationData().OrgName;
            var pageContent = await getPageQuery.GetPageBySlug(GroupsSchoolDashboardSlug, cancellationToken);
            List<ContentComponent> content = pageContent?.Content ?? new List<ContentComponent>();

            if (string.IsNullOrEmpty(schoolName))
            {
                return RedirectToAction("GetSelectASchoolView");
            }

            return _groupsRouter.GetSchoolDashboard(schoolId, schoolName, groupName, content, this, cancellationToken);
        }
    }
}
