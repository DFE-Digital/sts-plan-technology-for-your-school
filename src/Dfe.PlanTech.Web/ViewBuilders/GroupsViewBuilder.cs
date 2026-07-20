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

    public async Task<IActionResult> RouteToSelectASchoolViewModelAsync(Controller controller)
    {
        // Get the user's organisation ID (the MAT/group), not the active establishment
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

        var groupSchools =
            await _establishmentService.GetEstablishmentLinksWithRecommendationCounts(
                establishmentId
            );

        var contactLink = await ContentfulService.GetLinkByIdAsync(_contactOptions.LinkId);

        var viewModel = new GroupsSelectorViewModel
        {
            GroupName = groupName,
            GroupEstablishments = groupSchools,
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
        var orderedCategories = ((await ContentfulService.GetPageBySlugAsync("home")).Content ?? [])
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

        // establishments for the MAT
        var matEstablishmentLinks =
            await _establishmentService.GetEstablishmentLinks(establishmentId) ?? [];

        // Get urn's from links (filter for distinct, white spaces).
        var matEstablishmentUrns = matEstablishmentLinks
            .Select(e => e.Urn)
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct()
            .ToArray();

        // Retrieve the actual establishments.
        var matEstablishments =
            await _establishmentService.GetEstablishmentsByReferencesAsync(matEstablishmentUrns)
            ?? [];

        // Get the ids of the establishments.
        var matEstablishmentIds = matEstablishments.Select(e => e.Id).Distinct().ToArray();

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
                            var uncompletedCount = Math.Max(
                                0,
                                matEstablishmentIds.Length - completedCount
                            );
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

        var establishmentLinks = await _establishmentService.GetEstablishmentLinks(establishmentId);

        if (establishmentLinks == null || establishmentLinks.Count == 0)
        {
            throw new InvalidDataException(
                $"Could not find linked establishments for group ID: {establishmentId}"
            );
        }

        var schoolSubmissions = await _groupService.GetGroupSubmissionInformationForSection(
            establishmentLinks,
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

        var establishmentLinks = await _establishmentService.GetEstablishmentLinks(
            userEstablishmentId
        );

        if (establishmentLinks == null || establishmentLinks.Count == 0)
        {
            throw new InvalidDataException(
                $"Could not find linked establishments for group ID: {userEstablishmentId}"
            );
        }

        var selectedRefs = viewModel.SelectedSchoolsRefs.Contains("all")
            ? viewModel.PresentedSchoolRefs.ToArray()
            : viewModel.SelectedSchoolsRefs.ToArray();

        if (selectedRefs.Length == 1)
        {
            var isGroupSchool = VerifyGroupSchoolMembership(selectedRefs[0], establishmentLinks);
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

                CurrentUser.SetGroupSelectedSchool(school.EstablishmentRef, school.OrgName);

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
                var isGroupSchool = VerifyGroupSchoolMembership(schoolRef, establishmentLinks);
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

    private bool VerifyGroupSchoolMembership(
        string schoolRef,
        List<SqlEstablishmentLinkDto> establishmentLinks
    )
    {
        var establishment = establishmentLinks.Find(est => est.Urn == schoolRef);

        if (establishment == null)
        {
            _logger.LogWarning(
                "Selected school with ref {SchoolRef} not linked to user's group",
                schoolRef
            );
            return false;
        }

        return true;
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
            GroupUid = CurrentUser.UserOrganisationUid, // TODO: resolve some confusion here - the database table is `GroupUid` and is populated from the `uid` OIDC claim - possibly remove `groupUid` from `EstablishmentModel`?
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
            await _establishmentService.GetEstablishmentsByReferencesAsync(
                matEstablishmentUrns
            ) ?? [];

        var matEstablishmentIds = matEstablishments
            .Select(e => e.Id)
            .Distinct()
            .ToArray();

        if (matEstablishmentIds.Length == 0)
        {
            return false;
        }

        var completedSubmissions =
            await _groupService.GetGroupCompletedSubmissionsBySections(
                matEstablishmentIds
            ) ?? [];

        var requiredSectionIds = sections
            .Select(s => s.Id)
            .Distinct()
            .ToHashSet();

        var completedSchoolSections = completedSubmissions
            .Where(s => requiredSectionIds.Contains(s.SectionId))
            .Select(s => (s.EstablishmentId, s.SectionId))
            .Distinct()
            .Count();

        var totalRequiredSchoolSections =
            matEstablishmentIds.Length * requiredSectionIds.Count;

        return completedSchoolSections < totalRequiredSchoolSections;
    }
}
