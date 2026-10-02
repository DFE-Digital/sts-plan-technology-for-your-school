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
    public const string GetSelectASchoolToUpdateStatusAction = "GetSelectASchoolToUpdateStatusView";
    public const string GetSelectASelfAssessmentAction = "GetSelectASelfAssessment";
    public const string GetSelectSchoolsToAssessAction = "GetSelectSchoolsToAssessView";
    public const string SubmitSchoolsSelectionAction = "SubmitSelectedSchoolsToAssess";
    public const string SubmitSchoolsUpdateStatusSelectionAction = "SubmitSelectedSchoolsToUpdateStatus";
    public const string GetMatStandardsListAction = "GetMatStandardsList";
    public const string GetMatRecommendationsLandingAction = "GetMatRecommendationsLanding";

    private readonly ICurrentUserProvider _currentUser;
    private readonly IGroupsViewBuilder _groupsViewBuilder;
    private readonly IGroupSelectSchoolsToAssessValidator _groupSelectSchoolsToAssessValidator;
    private readonly IGroupSelectSchoolsToUpdateStatusValidator _groupSelectSchoolsToUpdateStatusValidator;
    private readonly IPagesViewBuilder _pagesViewBuilder;

    public GroupsController(
        ILogger<GroupsController> logger,
        ICurrentUserProvider currentUser,
        IGroupsViewBuilder groupsViewBuilder,
        IGroupSelectSchoolsToAssessValidator groupSelectSchoolsToAssessValidator,
        IGroupSelectSchoolsToUpdateStatusValidator groupSelectSchoolsToUpdateStatusValidator,
        IPagesViewBuilder pagesViewBuilder
    )
        : base(logger)
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _groupsViewBuilder =
            groupsViewBuilder ?? throw new ArgumentNullException(nameof(groupsViewBuilder));
        _groupSelectSchoolsToAssessValidator = groupSelectSchoolsToAssessValidator ?? throw new ArgumentNullException(nameof(groupSelectSchoolsToAssessValidator));
        _groupSelectSchoolsToUpdateStatusValidator = groupSelectSchoolsToUpdateStatusValidator ?? throw new ArgumentNullException(nameof(groupSelectSchoolsToUpdateStatusValidator));

        _pagesViewBuilder =
            pagesViewBuilder ?? throw new ArgumentNullException(nameof(pagesViewBuilder));
    }

    [HttpGet(
        $"{UrlConstants.GroupsSlug}/{UrlConstants.GroupsSelectionPageSlug}",
        Name = GetSelectASchoolAction
    )]
    public async Task<IActionResult> GetSelectASchoolView()
    {
        HttpContext.Session.Remove(
            SessionConstants.SelectedEstablishmentsKey
        );

        return await _groupsViewBuilder.RouteToSelectASchoolViewModelAsync(this);
    }

    [HttpGet(
        $"{UrlConstants.GroupsSlug}/{UrlConstants.GroupSelfAssessmentSelectionSlug}",
        Name = GetSelectASelfAssessmentAction
    )]
    public async Task<IActionResult> GetSelectASelfAssessment()
    {
        var selectedEstablishmentIdsBeforeClear = HttpContext.Session.GetSelectedEstablishmentIds();

        _currentUser.ClearSelectedGroupSchool();

        HttpContext.Session.Remove(SessionConstants.SelectedEstablishmentsKey);

        return await _groupsViewBuilder.RouteToSelectASelfAssessmentViewModelAsync(this);
    }

    [HttpGet(
        $"{UrlConstants.GroupsSlug}/standards",
        Name = GetMatStandardsListAction
    )]
    public async Task<IActionResult> GetMatStandardsList()
    {
        return await _groupsViewBuilder.RouteToMatStandardsListAsync(this);
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

    [HttpGet(
        $"{UrlConstants.GroupsSlug}/{{categorySlug}}/{{sectionSlug}}/{{recommendationSlug}}/{UrlConstants.GroupsSelectSchoolsToUpdateStatusSlug}",
        Name = GetSelectASchoolToUpdateStatusAction
    )]
    public async Task<IActionResult> GetSelectSchoolsToUpdateView(string sectionSlug, string recommendationSlug)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(recommendationSlug);

        return await _groupsViewBuilder.RouteToSelectSchoolsToUpdateStatusViewModelAsync(
            this,
            sectionSlug,
            recommendationSlug
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

        var selectedEstablishmentIdsBeforeClear = HttpContext.Session.GetSelectedEstablishmentIds();

        await _groupsViewBuilder.RecordGroupSelectionAsync(schoolUrn, schoolName);

        HttpContext.Session.Remove(SessionConstants.SelectedEstablishmentsKey);

        _currentUser.SetGroupSelectedSchool(schoolUrn, schoolName);

        var selectedEstablishmentIdsAfterClear = HttpContext.Session.GetSelectedEstablishmentIds();

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

        await _groupSelectSchoolsToAssessValidator.ValidateSelectionAsync(viewModel, ModelState);

        if (!ModelState.IsValid)
        {
            return await _groupsViewBuilder.RouteToSelectSchoolsToAssessViewModelAsync(
                this,
                sectionSlug,
                viewModel
            );
        }

        var result = await _groupsViewBuilder.SubmitSelectedSchoolsToAssessAndRedirect(
            this,
            sectionSlug,
            viewModel
        );

        return result;
    }

    [HttpPost(
        $"{UrlConstants.GroupsSlug}/{{categorySlug}}/{{sectionSlug}}/{{recommendationSlug}}/{UrlConstants.GroupsSelectSchoolsToUpdateStatusSlug}",
        Name = SubmitSchoolsUpdateStatusSelectionAction
    )]
    public async Task<IActionResult> SubmitSelectedSchoolsToUpdateStatus(
        GroupsSelectSchoolsToUpdateStatusViewModel viewModel,
        string sectionSlug,
        string recommendationSlug
    )
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(recommendationSlug);

        await _groupSelectSchoolsToUpdateStatusValidator.ValidateSelectionAsync(viewModel, ModelState);

        if (!ModelState.IsValid)
        {
            return await _groupsViewBuilder.RouteToSelectSchoolsToUpdateStatusViewModelAsync(
                this,
                sectionSlug,
                recommendationSlug,
                viewModel
            );
        }

        //todo - handle path to select status page (different ticket).

        return Ok();
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
    public async Task<IActionResult> SelectSchool(string schoolUrn, string schoolName)
    {
        await _groupsViewBuilder.RecordGroupSelectionAsync(schoolUrn, schoolName);

        _currentUser.SetGroupSelectedSchool(schoolUrn, schoolName);

        return Redirect(UrlConstants.HomePage);
    }

    [HttpGet(
        $"{UrlConstants.GroupsSlug}/{{categorySlug}}",
        Name = GetMatRecommendationsLandingAction
    )]
    public async Task<IActionResult> GetMatRecommendationsLanding(string categorySlug)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(categorySlug);

        return await _pagesViewBuilder.RouteToMatCategoryLandingPageAsync(
            this,
            categorySlug
        );
    }
}
