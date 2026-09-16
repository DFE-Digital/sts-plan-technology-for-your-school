using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Configuration;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Data;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class GroupsViewBuilder(
    ILogger<GroupsViewBuilder> logger,
    IOptions<ContactOptionsConfiguration> contactOptions,
    IContentfulService contentfulService,
    ICurrentUserProvider currentUser,
    IEstablishmentService establishmentService,
    IGroupService groupService,
    ISubmissionService submissionService
) : BaseViewBuilder(logger, contentfulService, currentUser), IGroupsViewBuilder
{
    private readonly IEstablishmentService _establishmentService =
        establishmentService ?? throw new ArgumentNullException(nameof(establishmentService));

    private readonly IGroupService _groupService =
        groupService ?? throw new ArgumentNullException(nameof(groupService));

    private readonly ISubmissionService _submissionService =
        submissionService ?? throw new ArgumentNullException(nameof(submissionService));

    private readonly ContactOptionsConfiguration _contactOptions =
        contactOptions?.Value ?? throw new ArgumentNullException(nameof(contactOptions));

    private readonly ILogger<BaseViewBuilder> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    private const string SelectASchoolViewName = "GroupsSelectSchool";
    private const string SelectASelfAssessmentViewName = "GroupsSelectSelfAssessment";
    private const string SelectSchoolsToAssessViewName = "GroupSelectSchoolsToAssess";

    /// <summary>
    /// Called when first logging in as group to load basic info page containing selectable schools with count of in progress or complete recommendations per school.
    /// </summary>
    /// <param name="controller"></param>
    /// <returns></returns>
    /// <exception cref="ContentfulDataUnavailableException"></exception>
    public async Task<IActionResult> RouteToSelectASchoolViewModelAsync(Controller controller)
    {
        // Get the user's organisation ID (the MAT/group), not the active establishment
        // dbo.GroupMembership id
        // At this point, the user hasn't selected a school yet
        var establishmentId = GetUserOrganisationIdOrThrowException();

        var selectASchoolPageContent =
            await ContentfulService.GetPageBySlugAsync(UrlConstants.GroupsSelectionPageSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find contentful page for slug '{UrlConstants.GroupsSelectionPageSlug}'"
            );

        var groupName = CurrentUser.UserOrganisationName ?? "Your organisation";
        List<ContentfulEntry> content = selectASchoolPageContent.Content ?? [];

        var sections = await ContentfulService.GetAllSectionsAsync();
        var allRecommendations = sections.SelectMany(section => section.CoreRecommendations);
        string totalRecommendations = allRecommendations.Count().ToString();

        var showSelectSelfAssessmentToSubmit =
            await HasOutstandingSelfAssessmentsAsync(
                establishmentId,
                sections
            );

        var group = await groupService.GetGroupHomePageModel(establishmentId);

        var contactLink = await ContentfulService.GetLinkByIdAsync(_contactOptions.LinkId);

        var viewModel = new GroupsSelectorViewModel
        {
            GroupName = groupName,
            GroupEstablishments = group?.BasicEstablishments ?? [],
            BeforeTitleContent = selectASchoolPageContent.BeforeTitleContent ?? [],
            Title = new ComponentTitleEntry(groupName),
            Content = content,
            TotalRecommendations = totalRecommendations,
            ProgressRetrievalErrorMessage = string.IsNullOrEmpty(totalRecommendations)
                ? "Unable to retrieve progress"
                : null,
            ContactLinkHref = contactLink?.Href,
            ShowSelectSelfAssessmentToSubmit = showSelectSelfAssessmentToSubmit,
        };

        controller.ViewData[StatePassingMechanismConstants.Title] = "Select a school";
        return controller.View(SelectASchoolViewName, viewModel);
    }

    public async Task<IActionResult> RouteToViewInProgressAnswers(
        Controller controller,
        string categorySlug,
        string sectionSlug,
        string schoolUrn
    )
    {
        var schoolEstablishment = await _establishmentService.GetEstablishmentByReferenceAsync(
            schoolUrn
        );

        if (schoolEstablishment is null)
        {
            return controller.RedirectToHomePage();
        }

        var section =
            await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find section for slug {sectionSlug}"
            );

        var submissionRoutingData = await _submissionService.GetSubmissionRoutingDataAsync(
            schoolEstablishment.Id,
            section,
            status: SubmissionStatus.InProgress
        );

        switch (submissionRoutingData.Status)
        {
            case SubmissionStatus.InProgress:
            case SubmissionStatus.CompleteNotReviewed:
                var viewModel = ReviewAnswersViewBuilder.BuildViewAnswersViewModel(
                    section,
                    submissionRoutingData,
                    categorySlug,
                    sectionSlug,
                    isMatInProgressView: true,
                    schoolName: schoolEstablishment.OrgName
                );

                viewModel.BackLinkHref = $"/{categorySlug}/{sectionSlug}/self-assessment";

                return controller.View(ReviewAnswersViewBuilder.ViewAnswersViewName, viewModel);

            default:
                return controller.RedirectToHomePage();
        }
    }

    public async Task<IActionResult> RouteToSelectASelfAssessmentViewModelAsync(
        Controller controller
    )
    {
        // Get the MAT id
        var establishmentId = GetUserOrganisationIdOrThrowException();
        var groupName = CurrentUser.UserOrganisationName ?? "Your organisation";

        //Get all categories so e2e testing categories appear when enabled
        var allCategories = await ContentfulService.GetAllCategoriesAsync() ?? [];

        //Get ordered categories from home page
        var orderedCategories = ((await ContentfulService.GetPageBySlugAsync("home"))?.Content ?? [])
            .OfType<QuestionnaireCategoryEntry>()
            .ToList();

        var categories = orderedCategories
            .Concat(allCategories.ExceptBy(
                orderedCategories.Select(x => x.Sys?.Id),
                x => x.Sys?.Id))
            .ToList();

        if (categories.Count == 0)
        {
            throw new ContentfulDataUnavailableException(
                "No categories found on groups assessment selection page."
            );
        }

        // Get the completed submissions for the MAT.
        var group = await _groupService.GetGroupWithEstablishmentsFromGIASAndCreateInDbo(establishmentId);
        var matEstablishmentIds = group?.BasicEstablishments.Select(e => e.DboId).OfType<int>().ToList() ?? [];
        var totalSchools = matEstablishmentIds?.Count() ?? 0;
        var completedCountBySectionId = matEstablishmentIds != null && matEstablishmentIds.Any() ?
            await _groupService.GetGroupCompletedSubmissionCountBySection(matEstablishmentIds) : new Dictionary<string, int>();

        var viewModel = new GroupSelectAssessmentViewModel()
        {
            GroupName = groupName,
            Categories = categories
                .Select(c => new CategorySectionViewModel
                {
                    CategoryName = c.Header?.Text ?? string.Empty,
                    Sections = (c.Sections ?? [])
                        .Select(ccs =>
                        {
                            var completedCount = completedCountBySectionId.GetValueOrDefault(
                                ccs.Id
                            );
                            var uncompletedCount = totalSchools - completedCount;
                            return new GroupSelectAssessmentSectionViewModel()
                            {
                                SectionName = ccs.Name,
                                CategorySlug = c.Header?.Text?.Slugify(),
                                SectionSlug = ccs.InterstitialPage.Slug,
                                UncompletedGroupSubmissions = uncompletedCount,
                            };
                        })
                        .ToList(),
                })
                .ToList(),
        };

        return controller.View(SelectASelfAssessmentViewName, viewModel);
    }

    public async Task<IActionResult> RouteToSelectSchoolsToAssessViewModelAsync(
        Controller controller,
        string sectionSlug,
        GroupsSelectSchoolsToAssessViewModel? viewModel = null
    )
    {
        CurrentUser.ClearSelectedGroupSchool();
        controller.HttpContext.Session.Remove(SessionConstants.SelectedEstablishmentsKey);

        var categorySlug = controller.RouteData.Values["categorySlug"]?.ToString();
        var section =
            await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find topic for slug '{sectionSlug}'"
            );

        var establishmentId = GetUserOrganisationIdOrThrowException();

        var schoolSubmissions = await _groupService.GetGroupSubmissionInformationForSection(
            establishmentId,
            section.Id
        );

        var eligibleSchools = schoolSubmissions
            .Where(sub => sub.Status != SubmissionStatus.CompleteReviewed)
            .ToList();

        if (eligibleSchools.Count == 0)
        {
            return controller.RedirectToRoute(GroupsController.GetSelectASelfAssessmentAction);
        }

        viewModel ??= new GroupsSelectSchoolsToAssessViewModel();
        viewModel.CategorySlug = categorySlug;
        viewModel.Section = section;
        viewModel.SchoolSubmissionInfo = eligibleSchools;

        viewModel.ErrorMessages = controller
            .ModelState.Values.SelectMany(value => value.Errors.Select(err => err.ErrorMessage))
            .ToArray();

        return controller.View(SelectSchoolsToAssessViewName, viewModel);
    }

    public async Task<IActionResult> SubmitSelectedSchoolsToAssessAndRedirect(
        Controller controller,
        string sectionSlug,
        GroupsSelectSchoolsToAssessViewModel viewModel
    )
    {
        if (string.IsNullOrWhiteSpace(sectionSlug))
            throw new ArgumentNullException(nameof(sectionSlug));

        if (viewModel.SelectedSchoolsRefs == null || viewModel.SelectedSchoolsRefs.Count == 0)
            throw new InvalidDataException("No schools have been selected");

        var categorySlug =
            controller.RouteData.Values["categorySlug"]?.ToString()
            ?? throw new InvalidDataException("Missing category slug");

        var section =
            await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find topic for slug '{sectionSlug}'"
            );

        var userEstablishmentId = GetUserOrganisationIdOrThrowException();

        var selectedRefs = viewModel.SelectedSchoolsRefs.Contains("all")
            ? viewModel.PresentedSchoolRefs.ToArray()
            : viewModel.SelectedSchoolsRefs.ToArray();

        if (selectedRefs.Length == 1)
        {
            var isGroupSchool = await _groupService.IsSchoolWithinGroup(userEstablishmentId, selectedRefs[0]);
            if (!isGroupSchool)
            {
                throw new InvalidDataException(
                    $"Selected school with ref {selectedRefs[0]} not linked to user's group"
                );
            }
            else
            {
                var school =
                    await _establishmentService.GetEstablishmentByReferenceAsync(selectedRefs[0])
                    ?? throw new InvalidDataException(
                        $"School with ref {selectedRefs[0]} not found"
                    );

                if (
                    string.IsNullOrWhiteSpace(school.EstablishmentRef)
                    || string.IsNullOrWhiteSpace(school.OrgName)
                )
                {
                    throw new InvalidDataException(
                        $"School with ref {selectedRefs[0]} is missing required data"
                    );
                }

                // This has to remain due to user-action requiring it for the continue self-assessment page
                CurrentUser.SetGroupSelectedSchool(school.EstablishmentRef, school.OrgName);

                // This has been added, so that we know it has come through as a bulk assessment
                controller.HttpContext.Session.SetValue<IEnumerable<int>>(
                    SessionConstants.SelectedEstablishmentsKey,
                    new List<int> { school.Id }
                );

                var latestSubmissionForRef =
                    await _submissionService.GetLatestSubmissionResponsesModel(
                        school.Id,
                        section,
                        (SubmissionStatus?)null
                    );

                if (
                    latestSubmissionForRef != null
                    && latestSubmissionForRef.Status == SubmissionStatus.InProgress
                )
                {
                    return controller.RedirectToRoute(
                        QuestionsController.GetContinueSelfAssessmentAction,
                        new { categorySlug, sectionSlug }
                    );
                }
            }
        }
        else if (selectedRefs.Length > 1)
        {
            var selectedSchoolIds = new List<int>();

            foreach (var schoolRef in selectedRefs)
            {
                var isGroupSchool = await _groupService.IsSchoolWithinGroup(userEstablishmentId, schoolRef);
                if (isGroupSchool)
                {
                    var school = await _establishmentService.GetEstablishmentByReferenceAsync(
                        schoolRef
                    );

                    if (school != null)
                    {
                        var latestSubmissionForRef =
                            await _submissionService.GetLatestSubmissionResponsesModel(
                                school.Id,
                                section,
                                (SubmissionStatus?)null
                            );

                        if (
                            latestSubmissionForRef != null
                            && latestSubmissionForRef.Status == SubmissionStatus.InProgress
                        )
                        {
                            await _submissionService.SetSubmissionInaccessibleAsync(
                                school.Id,
                                section.Id
                            );
                        }

                        selectedSchoolIds.Add(school.Id);
                    }
                }
            }

            controller.HttpContext.Session.SetValue<IEnumerable<int>>(
                SessionConstants.SelectedEstablishmentsKey,
                selectedSchoolIds
            );
        }

        var questionSlug = section.Questions.First().Slug;

        return controller.RedirectToRoute(
            QuestionsController.GetQuestionBySlugAction,
            new
            {
                categorySlug,
                sectionSlug,
                questionSlug,
            }
        );
    }

    public async Task RecordGroupSelectionAsync(
        string selectedEstablishmentUrn,
        string selectedEstablishmentName
    )
    {
        var userDsiReference = GetDsiReferenceOrThrowException();
        var userOrganisationId = CurrentUser.UserOrganisationId;

        // Construct the user's organisation model from individual properties
        var userOrganisationModel = new EstablishmentModel
        {
            Id = CurrentUser.UserOrganisationDsiId ?? Guid.Empty,
            Name = CurrentUser.UserOrganisationName ?? string.Empty,
            Urn = CurrentUser.UserOrganisationUrn,
            Ukprn = CurrentUser.UserOrganisationUkprn,
            Uid = CurrentUser.UserOrganisationUid,
            GroupUid = CurrentUser
                .UserOrganisationUid, // TODO: resolve some confusion here - the database table is `GroupUid` and is populated from the `uid` OIDC claim - possibly remove `groupUid` from `EstablishmentModel`?
            Type = CurrentUser.UserOrganisationTypeName is null
                ? null
                : new IdWithNameModel { Name = CurrentUser.UserOrganisationTypeName },
        };

        await _establishmentService.RecordGroupSelection(
            userDsiReference,
            userOrganisationId,
            userOrganisationModel,
            selectedEstablishmentUrn,
            selectedEstablishmentName
        );
    }

    /// <summary>
    /// Compare number of all completed sections with how many there would be if every school in the MAT had completed every section.
    /// </summary>
    /// <param name="matEstablishmentId"></param>
    /// <param name="sections"></param>
    /// <returns></returns>
    private async Task<bool> HasOutstandingSelfAssessmentsAsync(
        int matEstablishmentId,
        IEnumerable<QuestionnaireSectionEntry> sections
    )
    {
        var group = await _groupService.GetGroupWithEstablishmentsFromGIASAndCreateInDbo(matEstablishmentId);
        var matEstablishmentIds = group?.BasicEstablishments.Select(e => e.DboId).OfType<int>().ToList() ?? [];
        var totalSchools = matEstablishmentIds?.Count() ?? 0;
        var completedCountBySectionId = matEstablishmentIds != null && matEstablishmentIds.Any() ?
            await _groupService.GetGroupCompletedSubmissionCountBySection(matEstablishmentIds) : new Dictionary<string, int>();

        var requiredSectionIds = sections
            .Select(s => s.Id)
            .Distinct()
            .ToHashSet();

        var completedSchoolSections = completedCountBySectionId.Sum(s => s.Value);

        var totalRequiredSchoolSections =
            totalSchools * requiredSectionIds.Count;

        return completedSchoolSections < totalRequiredSchoolSections;
    }
}
