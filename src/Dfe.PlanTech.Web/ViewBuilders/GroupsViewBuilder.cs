using Dfe.PlanTech.Application.Configuration;
using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class GroupsViewBuilder(
    ILogger<BaseViewBuilder> logger,
    IOptions<ContactOptionsConfiguration> contactOptions,
    CurrentUser currentUser,
    ContentfulService contentfulService,
    EstablishmentService establishmentService,
    SubmissionService submissionService
) : BaseViewBuilder(logger, contentfulService, currentUser)
{
    private readonly ContactOptionsConfiguration _contactOptions = contactOptions?.Value ?? throw new ArgumentNullException(nameof(contactOptions));
    private readonly EstablishmentService _establishmentService = establishmentService ?? throw new ArgumentNullException(nameof(establishmentService));
    private readonly SubmissionService _submissionService = submissionService ?? throw new ArgumentNullException(nameof(submissionService));

    private const string SelectASchoolViewName = "GroupsSelectSchool";
    private const string SchoolDashboardViewName = "GroupsSchoolDashboard";
    private const string SchoolRecommendationsViewName = "Recommendations";
    private const string RecommendationsChecklistViewName = "RecommendationsChecklist";

    public async Task<IActionResult> RouteToSelectASchoolViewModelAsync(Controller controller)
    {
        var establishmentId = GetEstablishmentIdOrThrowException();

        var selectASchoolPageContent = await ContentfulService.GetPageBySlugAsync(UrlConstants.GroupsSelectionPageSlug);
        var dashboardContent = await ContentfulService.GetPageBySlugAsync(UrlConstants.GroupsDashboardSlug);
        var categories = dashboardContent.Content?.OfType<QuestionnaireCategoryEntry>();

        if (categories is null)
        {
            throw new InvalidDataException("There are no categories to display for the selected page.");
        }

        var groupSchools = await _establishmentService.GetEstablishmentLinksWithSubmissionStatusesAndCounts(categories, establishmentId);

        var groupName = CurrentUser.GetEstablishmentModel().Name;
        var title = groupName;
        List<ContentfulEntry> content = selectASchoolPageContent?.Content ?? [];

        string totalSections = string.Empty;
        if (categories != null)
        {
            totalSections = categories.Sum(category => category.Sections.Count).ToString();
        }

        var contactLink = await ContentfulService.GetLinkByIdAsync(_contactOptions.LinkId);

        var viewModel = new GroupsSelectorViewModel
        {
            GroupName = groupName,
            GroupEstablishments = groupSchools,
            Title = new ComponentTitleEntry(title),
            Content = content,
            TotalSections = totalSections,
            ProgressRetrievalErrorMessage = String.IsNullOrEmpty(totalSections)
                ? "Unable to retrieve progress"
                : null,
            ContactLinkHref = contactLink?.Href
        };


        controller.ViewData["Title"] = "Select a school";
        return controller.View(SelectASchoolViewName, viewModel);
    }

    public async Task<int> RecordGroupSelectionAsync(string selectedEstablishmentUrn, string selectedEstablishmentName)
    {
        var userDsiReference = GetDsiReferenceOrThrowException();

        var selectionId = await _establishmentService.RecordGroupSelection(
            userDsiReference,
            CurrentUser.EstablishmentId,
            CurrentUser.GetEstablishmentModel(),
            selectedEstablishmentUrn,
            selectedEstablishmentName
        );

        return selectionId;
    }

    public async Task<IActionResult> RouteToSchoolDashboardViewAsync(Controller controller)
    {
        var latestSelection = await GetCurrentGroupSchoolSelection();
        var groupName = CurrentUser.GetEstablishmentModel().Name;
        var pageContent = await ContentfulService.GetPageBySlugAsync(UrlConstants.GroupsDashboardSlug);
        List<ContentfulEntry> content = pageContent?.Content ?? [];

        var viewModel = new GroupsSchoolDashboardViewModel
        {
            SchoolName = latestSelection.SelectedEstablishmentName,
            SchoolId = latestSelection.SelectedEstablishmentId,
            GroupName = groupName,
            Title = new ComponentTitleEntry("Plan technology for your school"),
            Content = content,
            Slug = UrlConstants.GroupsDashboardSlug
        };

        if (viewModel is null)
        {
            return controller.RedirectToAction(GroupsController.GetSchoolDashboardAction);
        }

        controller.ViewData["Title"] = "Dashboard";
        return controller.View(SchoolDashboardViewName, viewModel);
    }

    public async Task<IActionResult> RouteToGroupsRecommendationAsync(Controller controller, string sectionSlug)
    {
        var latestSelection = await GetCurrentGroupSchoolSelection();
        var schoolId = latestSelection.SelectedEstablishmentId;
        var schoolName = latestSelection.SelectedEstablishmentName;

        var viewModel = await GetGroupsRecommendationsViewModel(sectionSlug, schoolId, schoolName);

        if (viewModel is null)
        {
            return controller.RedirectToAction(GroupsController.GetSchoolDashboardAction);
        }

        // Passes the school name to the Header
        controller.ViewData["SelectedEstablishmentName"] = viewModel.SelectedEstablishmentName;
        controller.ViewData["Title"] = viewModel.SectionName;

        return controller.View(SchoolRecommendationsViewName, viewModel);
    }

    public async Task<IActionResult> RouteToRecommendationsPrintViewAsync(Controller controller, string sectionSlug, int schoolId, string schoolName)
    {
        var viewModel = await GetGroupsRecommendationsViewModel(sectionSlug, schoolId, schoolName);

        if (viewModel is null)
        {
            return controller.RedirectToAction(GroupsController.GetSchoolDashboardAction);
        }

        controller.ViewData["Title"] = viewModel.SectionName;
        return controller.View(RecommendationsChecklistViewName, viewModel);
    }

    private Task<SqlGroupReadActivityDto> GetCurrentGroupSchoolSelection()
    {
        var userId = GetUserIdOrThrowException();
        var establishmentId = GetEstablishmentIdOrThrowException();
        return _establishmentService.GetLatestSelectedGroupSchoolAsync(userId, establishmentId);
    }

    private async Task<GroupsRecommendationsViewModel?> GetGroupsRecommendationsViewModel(string sectionSlug, int schoolId, string schoolName)
    {
        var section = await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException($"Could not find section for slug: {sectionSlug}");

        var subtopicRecommendation = await ContentfulService.GetSubtopicRecommendationByIdAsync(section.Id)
            ?? throw new ContentfulDataUnavailableException($"Could not find subtopic recommendation for section {section.Name}");

        var latestResponses = await _submissionService.GetLatestSubmissionResponsesModel(schoolId, section, true)
            ?? throw new DatabaseException($"Could not find user's answers for section {section.Name}");

        var customIntro = new GroupsCustomRecommendationIntroViewModel()
        {
            HeaderText = $"{section.Name} recommendations",
            IntroContent = "The recommendations are based on the following answers provided by the school when they completed the self-assessment.",
            LinkText = "Overview",
            SelectedEstablishmentName = schoolName,
            Responses = latestResponses.Responses.ToList(),
        };

        if (subtopicRecommendation.Section is null)
        {
            return null;
        }

        var answerIds = latestResponses.Responses.Select(r => r.AnswerSysId);
        var subtopicChunks = subtopicRecommendation
            .Section
            .Chunks
            .Where(chunk => chunk.Answers.Exists(chunkAnswer => answerIds.Contains(chunkAnswer.Id)))
            .Distinct()
            .ToList();

        var viewModel = new GroupsRecommendationsViewModel
        {
            SectionName = subtopicRecommendation.Subtopic.Name,
            SelectedEstablishmentId = schoolId,
            SelectedEstablishmentName = schoolName,
            Slug = sectionSlug,
            Chunks = subtopicChunks,
            GroupsCustomRecommendationIntro = customIntro,
            SubmissionResponses = latestResponses.Responses
        };

        return viewModel;
    }
}
