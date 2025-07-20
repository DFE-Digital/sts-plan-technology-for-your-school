using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Domain.Groups.Models;
using Dfe.PlanTech.Web.Configurations;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class GroupsViewBuilder(
    IOptions<ContactOptionsConfiguration> contactOptions,
    CurrentUser currentUser,
    ContentfulService contentfulService,
    EstablishmentService establishmentService,
    SubmissionService submissionService
) : BaseViewBuilder(currentUser)
{
    private readonly ContactOptionsConfiguration _contactOptions = contactOptions?.Value ?? throw new ArgumentNullException(nameof(contactOptions));
    private readonly ContentfulService _contentfulService = contentfulService ?? throw new ArgumentNullException(nameof(contentfulService));
    private readonly EstablishmentService _establishmentService = establishmentService ?? throw new ArgumentNullException(nameof(establishmentService));
    private readonly SubmissionService _submissionService = submissionService ?? throw new ArgumentNullException(nameof(submissionService));

    public async Task<GroupsSelectorViewModel> GetSelectASchoolViewModelAsync(Controller controller)
    {
        var establishmentId = GetEstablishmentIdOrThrowException();

        var selectASchoolPageContent = await _contentfulService.GetPageBySlugAsync(UrlConstants.GroupsSelectionPageSlug);
        var dashboardContent = await _contentfulService.GetPageBySlugAsync(UrlConstants.GroupsDashboardSlug);
        var categories = dashboardContent.Content.OfType<CmsCategoryDto>();

        var groupSchools = await _establishmentService.BuildSchoolsWithSubmissionCountsView(categories, establishmentId);

        var groupName = CurrentUser.GetEstablishmentModel().OrgName;
        var title = groupName;
        List<CmsEntryDto> content = selectASchoolPageContent?.Content ?? [];

        string totalSections = string.Empty;
        if (categories != null)
        {
            totalSections = categories.Sum(category => category.Sections.Count).ToString();
        }

        var contactLink = await _contentfulService.GetLinkByIdAsync(_contactOptions.LinkId);

        var viewModel = new GroupsSelectorViewModel
        {
            GroupName = groupName,
            GroupEstablishments = groupSchools,
            Title = title,
            Content = content,
            TotalSections = totalSections,
            ProgressRetrievalErrorMessage = String.IsNullOrEmpty(totalSections)
                ? "Unable to retrieve progress"
                : null,
            ContactLinkHref = contactLink?.Href
        };

        return viewModel;
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

    public async Task<GroupsSchoolDashboardViewModel> GetSchoolDashboardViewAsync()
    {
        var latestSelection = await GetCurrentSelection();
        var groupName = CurrentUser.GetEstablishmentModel().OrgName;
        var pageContent = await _contentfulService.GetPageBySlugAsync(UrlConstants.GroupsDashboardSlug);
        List<CmsEntryDto> content = pageContent?.Content ?? [];

        var viewModel = new GroupsSchoolDashboardViewModel
        {
            SchoolName = latestSelection.SelectedEstablishmentName,
            SchoolId = latestSelection.SelectedEstablishmentId,
            GroupName = groupName,
            Title = new CmsComponentTitleDto() { Text = "Plan technology for your school" },
            Content = content,
            Slug = UrlConstants.GroupsDashboardSlug
        };

        return viewModel;
    }

    public async Task<GroupsRecommendationsViewModel> GetGroupsRecommendationAsync(string sectionSlug)
    {
        var latestSelection = await GetCurrentSelection();
        var schoolId = latestSelection.SelectedEstablishmentId;
        var schoolName = latestSelection.SelectedEstablishmentName;

        var section = await _contentfulService.GetSectionBySlugAsync(sectionSlug);
        var subTopicRecommendation = await _contentfulService.GetSubTopicRecommendation(section.Id)
            ?? throw new ContentfulDataUnavailableException($"Could not find subtopic recommendation for section: {section.Name}");

        var latestResponses = await _submissionService.GetLatestSubmissionWithResponsesAsync(schoolId, sectionSlug, true)
            ?? throw new DatabaseException($"Could not find user's answers for section: {section.Name}");

        var customIntro = new GroupsCustomRecommendationIntroViewModel()
        {
            HeaderText = $"{section.Name} recommendations",
            IntroContent = "The recommendations are based on the following answers provided by the school when they completed the self-assessment.",
            LinkText = "Overview",
            SelectedEstablishmentName = schoolName,
            Responses = latestResponses.Responses.ToList(),
        };

        var answerIds = latestResponses.Responses.Select(r => r.AnswerSysId);
        var subTopicChunks = subTopicRecommendation
            .Section
            .Chunks
            .Where(chunk => chunk.Answers.Exists(chunkAnswer => answerIds.Contains(chunkAnswer.Sys.Id)))
            .Distinct()
            .ToList();

        var viewModel = new GroupsRecommendationsViewModel
        {
            SectionName = subTopicRecommendation.Subtopic.Name,
            SelectedEstablishmentId = schoolId,
            SelectedEstablishmentName = schoolName,
            Slug = sectionSlug,
            Chunks = subTopicChunks,
            GroupsCustomRecommendationIntro = customIntro,
            SubmissionResponses = latestResponses.Responses
        };

        return viewModel;
    }

    private Task<SqlGroupReadActivityDto> GetCurrentSelection()
    {
        var userId = GetUserIdOrThrowException();
        var userEstablishmentId = GetEstablishmentIdOrThrowException();
        return _establishmentService.GetLatestSelectedGroupSchoolAsync(userId, userEstablishmentId);
    }
}
