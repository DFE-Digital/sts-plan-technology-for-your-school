using System.Data;
using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Configuration;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using EnumExtensions = Dfe.PlanTech.Core.Extensions.EnumExtensions;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class GroupsViewBuilder(
    ILogger<GroupsViewBuilder> logger,
    IOptions<ContactOptionsConfiguration> contactOptions,
    IContentfulService contentfulService,
    ICurrentUserProvider currentUser,
    IEstablishmentService establishmentService,
    IGroupService groupService,
    ISubmissionService submissionService,
    IRecommendationService recommendationService,
    IMicrocopyProvider microcopyProvider
) : BaseViewBuilder(logger, contentfulService, currentUser), IGroupsViewBuilder
{
    private readonly IEstablishmentService _establishmentService =
        establishmentService ?? throw new ArgumentNullException(nameof(establishmentService));

    private readonly IGroupService _groupService =
        groupService ?? throw new ArgumentNullException(nameof(groupService));

    private readonly ISubmissionService _submissionService =
        submissionService ?? throw new ArgumentNullException(nameof(submissionService));

    private readonly IRecommendationService _recommendationService =
        recommendationService ?? throw new ArgumentNullException(nameof(recommendationService));

    private readonly ContactOptionsConfiguration _contactOptions =
        contactOptions?.Value ?? throw new ArgumentNullException(nameof(contactOptions));

    private readonly IMicrocopyProvider _microcopyProvider =
        microcopyProvider ?? throw new ArgumentNullException(nameof(microcopyProvider));

    private readonly ILogger<BaseViewBuilder> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    private const string SelectASchoolViewName = "GroupsSelectSchool";
    private const string SelectASelfAssessmentViewName = "GroupsSelectSelfAssessment";
    private const string SelectSchoolsToAssessViewName = "GroupSelectSchoolsToAssess";
    private const string SelectSchoolsToUpdateStatusViewName = "GroupsSelectSchoolsToUpdateStatus";
    private const string SelectStatusToUpdateViewName = "GroupsSelectStatusToUpdate";

    public async Task<IActionResult> RouteToSelectASchoolViewModelAsync(Controller controller)
    {
        // Get the user's organisation ID (the MAT/group), not the active establishment
        // At this point, the user hasn't selected a school yet

        var establishment = CurrentUser.Establishment;

        var selectASchoolPageContent =
            await ContentfulService.GetPageBySlugAsync(UrlConstants.GroupsSelectionPageSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find contentful page for slug '{UrlConstants.GroupsSelectionPageSlug}'"
            );

        var groupName = establishment.OrgName ?? "Your organisation";
        List<ContentfulEntry> content = selectASchoolPageContent.Content ?? [];

        var sections = await ContentfulService.GetAllSectionsAsync();
        var allRecommendations = sections.SelectMany(section => section.CoreRecommendations);
        string totalRecommendations = allRecommendations.Count().ToString();

        var showSelectSelfAssessmentToSubmit = await HasOutstandingSelfAssessmentsAsync(
            establishment.Id,
            sections
        );

        var contactLink = await ContentfulService.GetLinkByIdAsync(_contactOptions.LinkId);
        var groupSchools = CurrentUser.GetGroupSchools();

        var viewModel = new GroupsSelectorViewModel
        {
            GroupName = groupName,
            GroupEstablishments = groupSchools.ToList(),
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

                return controller.View(ReviewAnswersViewBuilder.ViewAnswersViewName, viewModel);

            default:
                return controller.RedirectToHomePage();
        }
    }

    public async Task<IActionResult> RouteToSelectSchoolsToUpdateStatusViewModelAsync(
        Controller controller,
        string sectionSlug,
        string recommendationSlug,
        GroupsSelectSchoolsToUpdateStatusViewModel? viewModel = null
    )
    {
        CurrentUser.ClearSelectedSchools();

        var matEstablishmentId = CurrentUser.Establishment.Id;

        var categorySlug = controller.RouteData.Values["categorySlug"]?.ToString();

        var section =
            await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find section for slug {sectionSlug}"
            );
        var recommendationChunk =
            section.CoreRecommendations.FirstOrDefault(r =>
                r.Slug.Equals(recommendationSlug, StringComparison.OrdinalIgnoreCase)
            )
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find recommendation for slug {recommendationSlug}"
            );

        var dbRecommendation =
            await _recommendationService.GetRecommendationsByContentfulReferencesAsync([
                recommendationChunk.Id,
            ]);

        var recommendation =
            dbRecommendation.OrderByDescending(r => r.Id).FirstOrDefault()
            ?? throw new ArgumentException("Could not find recommendation");

        var latestEstRecHistory =
            await _groupService.GetLatestGroupEstablishmentRecommendationHistoryByRecommendationId(
                matEstablishmentId,
                recommendation.Id
            );

        var latestEstRecHistoryWithRecs = latestEstRecHistory
            .Where(d => d.recommendationHistory is not null)
            .ToList();

        var establishmentsRecommendations = latestEstRecHistoryWithRecs
            .Select(d => new EstablishmentRecommendationInfoModel()
            {
                EstablishmentId = d.establishment.Id,
                EstablishmentName = d.establishment.OrgName ?? string.Empty,
                EstablishmentRef = d.establishment.EstablishmentRef,
                SectionId = section.Id,
                EstablishmentRecommendationHistoryId = d.recommendationHistory!.Id,
                DateLastUpdated = DateTimeHelper.FormattedDateShort(
                    d.recommendationHistory.DateCreated
                ),
                Status = d.recommendationHistory.NewStatus ?? RecommendationStatus.NotStarted,
            })
            .ToList();

        viewModel ??= new GroupsSelectSchoolsToUpdateStatusViewModel();

        viewModel.Section = section;
        viewModel.EstablishmentsRecommendation = establishmentsRecommendations;
        viewModel.CategorySlug = categorySlug;
        viewModel.RecommendationChunk = recommendationChunk;

        viewModel.ErrorMessages = controller
            .ModelState.Values.SelectMany(value => value.Errors.Select(err => err.ErrorMessage))
            .ToArray();

        return controller.View(SelectSchoolsToUpdateStatusViewName, viewModel);
    }

    public async Task<IActionResult> RouteToSelectStatusToUpdateViewModelAsync(
        Controller controller,
        string sectionSlug,
        string recommendationSlug,
        GroupsSelectStatusToUpdateViewModel? viewModel = null
    )
    {
        var categorySlug = controller.RouteData.Values["categorySlug"]?.ToString();

        if (viewModel?.SelectedSchoolsRefs == null || viewModel.SelectedSchoolsRefs.Count == 0)
        {
            return await RouteToSelectSchoolsToUpdateStatusViewModelAsync(
                controller,
                sectionSlug,
                recommendationSlug,
                new GroupsSelectSchoolsToUpdateStatusViewModel()
            );
        }

        var section =
            await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find section for slug {sectionSlug}"
            );
        var recommendationChunk =
            section.CoreRecommendations.FirstOrDefault(r =>
                r.Slug.Equals(recommendationSlug, StringComparison.OrdinalIgnoreCase)
            )
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find recommendation for slug {recommendationSlug}"
            );

        var schools = await _establishmentService.GetEstablishmentsByReferencesAsync(
            viewModel.SelectedSchoolsRefs
        );

        var selectedSchools = CurrentUser.GetSelectedSchools();

        foreach (var selectedSchool in selectedSchools)
        {
            if (selectedSchool.LatestSubmissionStatus == SubmissionStatus.InProgress)
            {
                await _submissionService.SetSubmissionInaccessibleAsync(
                    selectedSchool.EstablishmentId,
                    section.Id
                );
            }
        }

        CurrentUser.SetSelectedSchools(selectedSchools);

        viewModel.StatusOptions = Enum.GetValues<RecommendationStatus>()
            .ToDictionary(key => key, key => EnumExtensions.GetDisplayName(key));
        viewModel.CurrentChunk = recommendationChunk;
        viewModel.Section = section;
        viewModel.SectionSlug = sectionSlug;
        viewModel.CategorySlug = categorySlug;
        viewModel.SelectedSchoolNames = schools.Select(s => s.OrgName).ToList();

        controller.ViewData[StatePassingMechanismConstants.Title] =
            "Select status for recommendation";

        return controller.View(SelectStatusToUpdateViewName, viewModel);
    }

    public async Task<IActionResult> RouteToSelectASelfAssessmentViewModelAsync(
        Controller controller
    )
    {
        //Get all categories so e2e testing categories appear when enabled
        var allCategories = await ContentfulService.GetAllCategoriesAsync() ?? [];

        //Get ordered categories from home page
        var orderedCategories = (
            (await ContentfulService.GetPageBySlugAsync("home"))?.Content ?? []
        )
            .OfType<QuestionnaireCategoryEntry>()
            .ToList();

        var categories = orderedCategories
            .Concat(
                allCategories.ExceptBy(orderedCategories.Select(x => x.Sys?.Id), x => x.Sys?.Id)
            )
            .ToList();

        if (categories.Count == 0)
        {
            throw new ContentfulDataUnavailableException(
                "No categories found on groups assessment selection page."
            );
        }

        var matEstablishmentIds = CurrentUser
            .GetGroupSchools()
            .Select(gs => gs.EstablishmentId)
            .ToArray();

        // Get the completed submissions for the MAT.
        var completedSubmissions =
            matEstablishmentIds.Length != 0
                ? await _groupService.GetGroupCompletedSubmissionsBySections(matEstablishmentIds)
                    ?? []
                : [];

        var completedCountBySectionId = completedSubmissions
            .Where(cs => matEstablishmentIds.Contains(cs.EstablishmentId))
            .GroupBy(cs => cs.SectionId)
            .ToDictionary(g => g.Key, g => g.Select(cs => cs.EstablishmentId).Distinct().Count());

        var viewModel = new GroupSelectAssessmentViewModel()
        {
            GroupName = CurrentUser.Establishment.OrgName ?? "Your organisation",
            Categories = categories
                .Select(c => new CategorySectionViewModel
                {
                    CategoryName = c.Header?.Text ?? string.Empty,
                    Sections = (c.Sections ?? [])
                        .Select(section =>
                        {
                            var completedCount = completedCountBySectionId.GetValueOrDefault(
                                section.Id
                            );
                            var uncompletedCount = Math.Max(
                                0,
                                matEstablishmentIds.Length - completedCount
                            );
                            return new GroupSelectAssessmentSectionViewModel()
                            {
                                SectionName = section.Name,
                                CategorySlug = c.LandingPage?.Slug,
                                SectionSlug = section.InterstitialPage.Slug,
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
        CurrentUser.ClearSelectedSchools();

        var categorySlug = controller.RouteData.Values["categorySlug"]?.ToString();
        var section =
            await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find topic for slug '{sectionSlug}'"
            );

        var groupSchools = CurrentUser.GetGroupSchools();

        var schoolSubmissions = await _groupService.GetGroupSubmissionInformationForSection(
            groupSchools,
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

        List<SelectedEstablishmentDataModel> selectedSchools =
            await GetSelectedSchoolsWithSubmissionStatuses(section);

        // If it's a single school with a submission in progress, redirect to it
        if (
            selectedSchools.Count == 1
            && selectedSchools.Single().LatestSubmissionStatus == SubmissionStatus.InProgress
        )
        {
            return controller.RedirectToRoute(
                QuestionsController.GetContinueSelfAssessmentAction,
                new { categorySlug, sectionSlug }
            );
        }

        foreach (var selectedSchool in selectedSchools)
        {
            if (selectedSchool.LatestSubmissionStatus == SubmissionStatus.InProgress)
            {
                await _submissionService.SetSubmissionInaccessibleAsync(
                    selectedSchool.EstablishmentId,
                    section.Id
                );
            }
        }

        CurrentUser.SetSelectedSchools(selectedSchools);

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
        int selectedEstablishmentId,
        string selectedEstablishmentUrn,
        string selectedEstablishmentName
    )
    {
        await _establishmentService.RecordGroupSelection(
            CurrentUser.UserId,
            CurrentUser.Establishment.Id,
            selectedEstablishmentId,
            selectedEstablishmentUrn,
            selectedEstablishmentName
        );
    }

    public async Task<IActionResult> RouteToMatStandardsListAsync(Controller controller)
    {
        CurrentUser.ClearSelectedSchools();

        var allCategories = await ContentfulService.GetAllCategoriesAsync() ?? [];
        var homePage = await ContentfulService.GetPageBySlugAsync("home");

        var orderedCategories = (homePage?.Content ?? [])
            .OfType<QuestionnaireCategoryEntry>()
            .ToList();

        var categories = orderedCategories
            .Concat(
                allCategories.ExceptBy(orderedCategories.Select(x => x.Sys?.Id), x => x.Sys?.Id)
            )
            .ToList();

        if (categories.Count == 0)
        {
            throw new ContentfulDataUnavailableException(
                "No categories found on MAT standards list page."
            );
        }

        var viewModel = new MatStandardsListViewModel
        {
            GroupName = CurrentUser.Establishment.OrgName ?? "Your organisation",
            Categories = categories,
        };

        controller.ViewData[StatePassingMechanismConstants.Title] = "Select a standard";

        return controller.View("MatStandardsList", viewModel);
    }

    public async Task<IActionResult> UpdateSchoolsRecommendationStatusAsync(
        Controller controller,
        string sectionSlug,
        string recommendationSlug,
        GroupRecommendationInputViewModel viewModel
    )
    {
        var categorySlug =
            controller.RouteData.Values["categorySlug"]?.ToString()
            ?? throw new InvalidDataException("Missing category slug");

        var userId = CurrentUser.UserId;
        var matEstablishmentId = CurrentUser.Establishment.Id;
        var selectedSchoolIds = CurrentUser.GetSelectedSchoolEstablishmentIds();
        var newStatus = viewModel.SelectedStatus.ToRecommendationStatus();
        if (newStatus is null)
        {
            throw new ArgumentException("Invalid status selected.");
        }

        var section =
            await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find section for slug {sectionSlug}"
            );
        var recommendationChunk =
            section.CoreRecommendations.FirstOrDefault(r =>
                r.Slug.Equals(recommendationSlug, StringComparison.OrdinalIgnoreCase)
            )
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find recommendation for slug {recommendationSlug}"
            );

        try
        {
            await _recommendationService.UpdateEstablishmentsRecommendationStatusAsync(
                recommendationChunk.Id,
                selectedSchoolIds,
                userId,
                newStatus.Value,
                viewModel.Notes,
                matEstablishmentId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update recommendation status for selected schools");

            return await RouteToSelectStatusToUpdateViewModelAsync(
                controller,
                sectionSlug,
                recommendationSlug,
                new GroupsSelectStatusToUpdateViewModel
                {
                    SelectedStatusKey = newStatus.Value,
                    SelectedSchoolsRefs = viewModel.SelectedSchoolsRefs,
                    StatusErrorMessage =
                        "There was a problem updating the recommendation status. Try again.",
                }
            );
        }

        var schoolsCount = selectedSchoolIds.Count;
        var dynamicValues = new Dictionary<string, string>()
        {
            ["schoolCount"] = schoolsCount.ToString(),
        };

        var microcopySuccessBodyText = await _microcopyProvider.GetTextByKeyAsync(
            ContentfulMicrocopyConstants.GroupsUpdateStatusSuccessPanelText,
            dynamicValues
        );

        microcopySuccessBodyText =
            schoolsCount == 1
                ? microcopySuccessBodyText.Replace("schools", "school")
                : microcopySuccessBodyText;

        controller.TempData["StatusUpdateSuccessTitle"] = microcopySuccessBodyText;

        return controller.RedirectToGetMatSingleRecommendation(
            categorySlug,
            sectionSlug,
            recommendationSlug
        );
    }

    private async Task<bool> HasOutstandingSelfAssessmentsAsync(
        int matEstablishmentId,
        IEnumerable<QuestionnaireSectionEntry> sections
    )
    {
        var matEstablishmentLinks =
            await _establishmentService.GetEstablishmentLinks(matEstablishmentId) ?? [];

        var matEstablishmentUrns = matEstablishmentLinks
            .Select(e => e.Urn)
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct()
            .ToArray();

        var matEstablishments =
            await _establishmentService.GetEstablishmentsByReferencesAsync(matEstablishmentUrns)
            ?? [];

        var matEstablishmentIds = matEstablishments.Select(e => e.Id).Distinct().ToArray();

        if (matEstablishmentIds.Length == 0)
        {
            return false;
        }

        var completedSubmissions =
            await _groupService.GetGroupCompletedSubmissionsBySections(matEstablishmentIds) ?? [];

        var requiredSectionIds = sections.Select(s => s.Id).Distinct().ToHashSet();

        var completedSchoolSections = completedSubmissions
            .Where(s => requiredSectionIds.Contains(s.SectionId))
            .Select(s => (s.EstablishmentId, s.SectionId))
            .Distinct()
            .Count();

        var totalRequiredSchoolSections = matEstablishmentIds.Length * requiredSectionIds.Count;

        return completedSchoolSections < totalRequiredSchoolSections;
    }

    private async Task<
        List<SelectedEstablishmentDataModel>
    > GetSelectedSchoolsWithSubmissionStatuses(QuestionnaireSectionEntry section)
    {
        var selectedSchools = CurrentUser.GetSelectedSchools();

        foreach (var school in selectedSchools)
        {
            var latestSubmissionForRef = await _submissionService.GetLatestSubmissionResponsesModel(
                school.EstablishmentId,
                section,
                (SubmissionStatus?)null
            );

            school.LatestSubmissionStatus = latestSubmissionForRef?.Status;
        }

        return selectedSchools;
    }
}
