using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Web.ViewBuilders;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Route("/")]
public class GroupsController : BaseController<GroupsController>
{
    public const string GetGroupsRecommendationAction = "GetGroupsRecommendation";
    public const string GetSchoolDashboardAction = "GetSchoolDashboard";
    private const string SelectASchoolViewName = "~/Views/Groups/GroupsSelectSchool.cshtml";
    private const string SchoolDashboardViewName = "~/Views/Groups/GroupsSchoolDashboard.cshtml";
    private const string SchoolRecommendationsViewName = "~/Views/Groups/Recommendations.cshtml";

    private GroupsViewBuilder _groupsViewBuilder;

    public GroupsController(
        ILoggerFactory loggerFactory,
        GroupsViewBuilder groupsViewBuilder
    ) : base(loggerFactory)
    {
        _groupsViewBuilder = groupsViewBuilder ?? throw new ArgumentNullException(nameof(groupsViewBuilder));
    }

    [HttpGet($"{UrlConstants.GroupsSlug}/{UrlConstants.GroupsSelectionPageSlug}")]
    public async Task<IActionResult> GetSelectASchoolView()
    {
        var viewModel = await _groupsViewBuilder.GetSelectASchoolViewModelAsync(this);

        ViewData["Title"] = "Select a school";
        return View(SelectASchoolViewName, viewModel);
    }

    [HttpPost($"{UrlConstants.GroupsSlug}/{UrlConstants.GroupsSelectionPageSlug}")]
    public async Task<IActionResult> SelectSchool(string schoolUrn, string schoolName)
    {
        await _groupsViewBuilder.RecordGroupSelectionAsync(schoolUrn, schoolName);

        return RedirectToAction("GetSchoolDashboardView");
    }

    [HttpGet($"{UrlConstants.GroupsSlug}/{UrlConstants.GroupsDashboardSlug}", Name = GetSchoolDashboardAction)]
    public async Task<IActionResult> GetSchoolDashboardView()
    {
        var viewModel = await _groupsViewBuilder.GetSchoolDashboardViewAsync();
        if (viewModel is null)
        {
            return RedirectToAction(GetSchoolDashboardAction);
        }

        ViewData["Title"] = "Dashboard";
        return View(SchoolDashboardViewName, viewModel);
    }

    [HttpGet($"{UrlConstants.GroupsSlug}/recommendations/{{sectionSlug}}")]
    public async Task<IActionResult> GetGroupsRecommendation(string sectionSlug)
    {
        var viewModel = await _groupsViewBuilder.GetGroupsRecommendationAsync(sectionSlug);
        if (viewModel is null)
        {
            return RedirectToAction(GetSchoolDashboardAction);
        }

        // Passes the school name to the Header
        ViewData["SelectedEstablishmentName"] = viewModel.SelectedEstablishmentName;
        ViewData["Title"] = viewModel.SectionName;

        return View(SchoolRecommendationsViewName, viewModel);
    }

    [HttpGet("groups/recommendations/{sectionSlug}/print", Name = "GetRecommendationsPrintView")]
    public async Task<IActionResult> GetRecommendationsPrintView(int schoolId, string schoolName, string sectionSlug)
    {
        var viewModel = await _groupsViewBuilder.GetRecommendationsPrintViewAsync(sectionSlug, schoolId, schoolName);
        if (viewModel is null)
        {
            return RedirectToAction(GetSchoolDashboardAction);
        }

        ViewData["Title"] = viewModel.SectionName;
        return View("~/Views/Groups/RecommendationsChecklist.cshtml", viewModel);
    }
}
