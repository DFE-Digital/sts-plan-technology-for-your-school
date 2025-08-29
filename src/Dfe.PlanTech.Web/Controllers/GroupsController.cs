using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Web.Attributes;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.ViewBuilders;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Route("/")]
public class GroupsController : BaseController<GroupsController>
{
    public const string GetGroupsRecommendationAction = "GetGroupsRecommendation";
    public const string GetSchoolDashboardAction = "GetSchoolDashboard";

    private readonly CurrentUser _currentUser;
    private readonly GroupsViewBuilder _groupsViewBuilder;

    public GroupsController(
        ILogger<GroupsController> logger,
        CurrentUser currentUser,
        GroupsViewBuilder groupsViewBuilder
    ) : base(logger)
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _groupsViewBuilder = groupsViewBuilder ?? throw new ArgumentNullException(nameof(groupsViewBuilder));
    }

    [HttpGet($"{UrlConstants.GroupsSlug}/{UrlConstants.GroupsSelectionPageSlug}")]
    public async Task<IActionResult> GetSelectASchoolView()
    {
        return await _groupsViewBuilder.RouteToSelectASchoolViewModelAsync(this);
    }

    [HttpPost($"{UrlConstants.GroupsSlug}/{UrlConstants.GroupsSelectionPageSlug}")]
    public async Task<IActionResult> SelectSchool(string schoolUrn, string schoolName)
    {
        await _groupsViewBuilder.RecordGroupSelectionAsync(schoolUrn, schoolName);
        _currentUser.SetGroupSelectedSchool(schoolUrn);

        return RedirectToAction(nameof(GetSchoolDashboardView));
    }

    [HttpGet($"{UrlConstants.GroupsSlug}/{UrlConstants.GroupsDashboardSlug}", Name = GetSchoolDashboardAction)]
    public async Task<IActionResult> GetSchoolDashboardView()
    {
        return await _groupsViewBuilder.RouteToSchoolDashboardViewAsync(this);
    }

    [HttpGet($"{UrlConstants.GroupsSlug}/recommendations/{{sectionSlug}}")]
    public async Task<IActionResult> GetGroupsRecommendation(string sectionSlug)
    {
        return await _groupsViewBuilder.RouteToGroupsRecommendationAsync(this, sectionSlug);
    }

    [LogInvalidModelState]
    [HttpGet("groups/recommendations/{sectionSlug}/print", Name = "GetRecommendationsPrintView")]
    public async Task<IActionResult> GetRecommendationsPrintView(int schoolId, string schoolName, string sectionSlug)
    {
        return await _groupsViewBuilder.RouteToRecommendationsPrintViewAsync(this, sectionSlug, schoolId, schoolName);
    }
}
