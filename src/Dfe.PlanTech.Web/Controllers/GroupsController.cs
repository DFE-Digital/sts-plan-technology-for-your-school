using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Route("/")]
public class GroupsController : BaseController<GroupsController>
{
    public const string GetSelectASchoolAction = "GetSelectASchoolView";
    public const string GetSelectASelfAssessmentAction = "GetSelectASelfAssessment";

    private readonly ICurrentUser _currentUser;
    private readonly IGroupsViewBuilder _groupsViewBuilder;

    public GroupsController(
        ILogger<GroupsController> logger,
        ICurrentUser currentUser,
        IGroupsViewBuilder groupsViewBuilder
    )
        : base(logger)
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _groupsViewBuilder =
            groupsViewBuilder ?? throw new ArgumentNullException(nameof(groupsViewBuilder));
    }

    [HttpGet(
        $"{UrlConstants.GroupsSlug}/{UrlConstants.GroupsSelectionPageSlug}",
        Name = GetSelectASchoolAction
    )]
    public async Task<IActionResult> GetSelectASchoolView()
    {
        return await _groupsViewBuilder.RouteToSelectASchoolViewModelAsync(this);
    }

    [HttpGet(
        $"{UrlConstants.GroupsSlug}/{UrlConstants.GroupSelfAssessmentSelectionSlug}",
        Name = GetSelectASelfAssessmentAction
    )]
    public async Task<IActionResult> GetSelectASelfAssessment()
    {
        return await _groupsViewBuilder.RouteToSelectASelfAssessmentViewModelAsync(this);
    }

    [HttpPost($"{UrlConstants.GroupsSlug}/{UrlConstants.GroupsSelectionPageSlug}")]
    public async Task<IActionResult> SelectSchool(string schoolUrn, string schoolName)
    {
        await _groupsViewBuilder.RecordGroupSelectionAsync(schoolUrn, schoolName);
        _currentUser.SetGroupSelectedSchool(schoolUrn, schoolName);

        return Redirect(UrlConstants.HomePage);
    }

    [HttpGet(
        $"school/{{categorySlug}}/{{sectionSlug}}/self-assessment/{UrlConstants.ViewAnswersSlug}"
    )]
    public async Task<IActionResult> ViewInProgressAnswers(
        string categorySlug,
        string sectionSlug
    )
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(categorySlug);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug);

        return await _groupsViewBuilder.RouteToViewInProgressAnswers(
            this,
            categorySlug,
            sectionSlug
        );
    }
}
