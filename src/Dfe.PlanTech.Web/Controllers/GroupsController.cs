using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Web.Validators.Interfaces;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Route("/")]
public class GroupsController : BaseController<GroupsController>
{
    public const string GetSelectASchoolAction = "GetSelectASchoolView";
    public const string GetSelectASelfAssessmentAction = "GetSelectASelfAssessment";
    public const string GetSelectSchoolsToAssessAction = "GetSelectSchoolsToAssessView";
    public const string SubmitSchoolsSelectionAction = "SubmitSelectedSchoolsToAssess";

    private readonly ICurrentUserProvider _currentUser;
    private readonly IGroupsViewBuilder _groupsViewBuilder;
    private readonly IGroupSelectSchoolsToAssessValidator _validator;

    public GroupsController(
        ILogger<GroupsController> logger,
        ICurrentUserProvider currentUser,
        IGroupsViewBuilder groupsViewBuilder,
        IGroupSelectSchoolsToAssessValidator validator
    )
        : base(logger)
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _groupsViewBuilder =
            groupsViewBuilder ?? throw new ArgumentNullException(nameof(groupsViewBuilder));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
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
        _currentUser.ClearSelectedGroupSchool();

        HttpContext.Session.Remove(
            SessionConstants.SelectedEstablishmentsKey
        );

        return await _groupsViewBuilder.RouteToSelectASelfAssessmentViewModelAsync(this);
    }

    [HttpGet(
        $"{UrlConstants.GroupsSlug}/{{categorySlug}}/{{sectionSlug}}/self-assessment/{UrlConstants.GroupsSelectSchoolsToAssessSlug}",
        Name = GetSelectSchoolsToAssessAction
    )]
    public async Task<IActionResult> GetSelectSchoolsToAssessView(string sectionSlug)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug);

        return await _groupsViewBuilder.RouteToSelectSchoolsToAssessViewModelAsync(
            this,
            sectionSlug
        );
    }

    [HttpGet($"{UrlConstants.GroupsSlug}/select-school-and-redirect")]
    public async Task<IActionResult> SelectSchoolAndRedirect(
    string schoolUrn,
    string schoolName,
    string categorySlug
)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schoolUrn);
        ArgumentException.ThrowIfNullOrWhiteSpace(schoolName);
        ArgumentException.ThrowIfNullOrWhiteSpace(categorySlug);

        await _groupsViewBuilder.RecordGroupSelectionAsync(
            schoolUrn,
            schoolName
        );

        _currentUser.SetGroupSelectedSchool(
            schoolUrn,
            schoolName
        );

        return RedirectToAction(
            nameof(PagesController.GetByRoute),
            nameof(PagesController).GetControllerNameSlug(),
            new { route = categorySlug }
        );
    }

    [HttpPost(
        $"{UrlConstants.GroupsSlug}/{{categorySlug}}/{{sectionSlug}}/self-assessment/{UrlConstants.GroupsSelectSchoolsToAssessSlug}",
        Name = SubmitSchoolsSelectionAction
    )]
    public async Task<IActionResult> SubmitSelectedSchoolsToAssess(
        GroupsSelectSchoolsToAssessViewModel viewModel,
        string sectionSlug
    )
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug);

        await _validator.ValidateSelectionAsync(viewModel, ModelState);

        if (!ModelState.IsValid)
        {
            return await _groupsViewBuilder.RouteToSelectSchoolsToAssessViewModelAsync(
                this,
                sectionSlug,
                viewModel
            );
        }

        return await _groupsViewBuilder.SubmitSelectedSchoolsToAssessAndRedirect(
            this,
            sectionSlug,
            viewModel
        );
    }

    [HttpGet(
        $"school/{{categorySlug}}/{{sectionSlug}}/self-assessment/{UrlConstants.ViewAnswersSlug}"
    )]
    public async Task<IActionResult> ViewInProgressAnswers(
        string categorySlug,
        string sectionSlug,
        string schoolUrn
    )
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(categorySlug);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(schoolUrn);

        return await _groupsViewBuilder.RouteToViewInProgressAnswers(
            this,
            categorySlug,
            sectionSlug,
            schoolUrn
        );
    }

    [HttpPost($"{UrlConstants.GroupsSlug}/{UrlConstants.GroupsSelectionPageSlug}")]
    public async Task<IActionResult> SelectSchool(
        string schoolUrn,
        string schoolName
    )
    {
        await _groupsViewBuilder.RecordGroupSelectionAsync(
            schoolUrn,
            schoolName
        );

        _currentUser.SetGroupSelectedSchool(
            schoolUrn,
            schoolName
        );

        return Redirect(UrlConstants.HomePage);
    }
}
