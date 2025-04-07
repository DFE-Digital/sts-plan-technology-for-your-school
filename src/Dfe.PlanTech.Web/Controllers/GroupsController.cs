using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Middleware;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers
{
    public class GroupsController : BaseController<GroupsController>
    {
        public const string GroupsSlug = "groups";
        public const string GroupsSelectorPageSlug = "select-a-school";
        public const string GroupsSchoolDashboardSlug = "dashboard";
        public const string GetSchoolDashboardAction = "GetSchoolDashboard";
        private const string selectASchoolViewName = "~/Views/Groups/GroupsSelectSchool.cshtml";
        private const string schoolDashboardViewName = "~/Views/Groups/GroupsSchoolDashboard.cshtml";

        private readonly IExceptionHandlerMiddleware _exceptionHandler;
        private readonly IUser _user;
        private readonly ILogger _logger;
        private readonly IGetEstablishmentIdQuery _getEstablishmentIdQuery;
        private readonly IGetSubmissionStatusesQuery _getSubmissionStatusesQuery;

        public GroupsController(ILogger<GroupsController> logger, IExceptionHandlerMiddleware exceptionHandler, IUser user, IGetEstablishmentIdQuery getEstablishmentIdQuery, IGetSubmissionStatusesQuery getSubmissionStatusesQuery) : base(logger)
        {
            _logger = logger;
            _exceptionHandler = exceptionHandler;
            _user = user;
            _getEstablishmentIdQuery = getEstablishmentIdQuery;
            _getSubmissionStatusesQuery = getSubmissionStatusesQuery;
        }

        [HttpGet($"{GroupsSlug}/{GroupsSelectorPageSlug}")]
        public async Task<IActionResult> GetSelectASchoolView([FromServices] IGetPageQuery getPageQuery, CancellationToken cancellationToken)
        {
            var selectASchoolPageContent = await getPageQuery.GetPageBySlug(GroupsSelectorPageSlug, cancellationToken);

            var schools = await _user.GetGroupEstablishments();
            var groupName = _user.GetOrganisationData().OrgName;
            var title = new Title() { Text = groupName };
            List<ContentComponent> content = selectASchoolPageContent?.Content ?? new List<ContentComponent>();

            var groupsViewModel = new GroupsSelectorViewModel()
            {
                GroupName = groupName,
                GroupEstablishments = schools,
                Title = title,
                Content = content
            };

            return View(selectASchoolViewName, groupsViewModel);
        }

        [HttpPost($"{GroupsSlug}/{GroupsSelectorPageSlug}")]
        public IActionResult SelectSchool(string schoolUrn, string schoolName)
        {
            TempData["SelectedSchoolUrn"] = schoolUrn;
            TempData["SelectedSchoolName"] = schoolName;

            return RedirectToAction("GetSchoolDashboardView");
        }

        [HttpGet($"{GroupsSlug}/{GroupsSchoolDashboardSlug}", Name = GetSchoolDashboardAction)]
        public async Task<IActionResult> GetSchoolDashboardView([FromServices] IGetPageQuery getPageQuery, CancellationToken cancellationToken)
        {
            string? schoolUrn = TempData["SelectedSchoolUrn"] as string;
            string? schoolName = TempData["SelectedSchoolName"] as string;

            if (string.IsNullOrEmpty(schoolName) || string.IsNullOrEmpty(schoolUrn))
            {
                return RedirectToAction("GetSelectASchoolView");
            }

            var schoolId = await _getEstablishmentIdQuery.GetEstablishmentId(schoolUrn);
            var groupName = _user.GetOrganisationData().OrgName;
            var pageContent = await getPageQuery.GetPageBySlug(GroupsSchoolDashboardSlug, cancellationToken);
            List<ContentComponent> content = pageContent?.Content ?? new List<ContentComponent>();

            var dashboardViewModel = new GroupsSchoolDashboardViewModel()
            {
                SchoolName = schoolName,
                SchoolId = (int)schoolId,
                GroupName = groupName,
                Title = new Title() { Text = "Plan technology for your school" },
                Content = content,
                Slug = GroupsSchoolDashboardSlug
            };
            
            return View(schoolDashboardViewName, dashboardViewModel);
        }
    }
}
