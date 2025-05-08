using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Establishments.Models;
using Dfe.PlanTech.Domain.Groups.Interfaces;
using Dfe.PlanTech.Domain.Groups.Models;
using Dfe.PlanTech.Domain.Helpers;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Configuration;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.Controllers
{
    [Route("/")]
    public class GroupsController : BaseController<GroupsController>
    {
        public const string GroupsSlug = UrlConstants.GroupsSlug;
        public const string GroupsSelectorPageSlug = UrlConstants.GroupsSelectionPageSlug;
        public const string GroupsSchoolDashboardSlug = UrlConstants.GroupsDashboardSlug;
        public const string GetSchoolDashboardAction = "GetSchoolDashboard";
        private const string selectASchoolViewName = "~/Views/Groups/GroupsSelectSchool.cshtml";
        private const string schoolDashboardViewName = "~/Views/Groups/GroupsSchoolDashboard.cshtml";
        public const string GetGroupsRecommendationAction = "GetGroupsRecommendation";

        private readonly ILogger _logger;
        private readonly IUser _user;
        private readonly IGetEstablishmentIdQuery _getEstablishmentIdQuery;
        private readonly IGetSubmissionStatusesQuery _getSubmissionStatusesQuery;
        private readonly IGetGroupSelectionQuery _getGroupSelectionQuery;
        private readonly IGetSectionQuery _getSectionQuery;
        private readonly IGetLatestResponsesQuery _getLatestResponsesQuery;
        private readonly IGetSubTopicRecommendationQuery _getSubTopicRecommendationQuery;
        private readonly IOptions<ContactOptions> _contactOptions;
        private readonly IGetNavigationQuery _getNavigationQuery;

        public GroupsController(ILogger<GroupsController> logger, IUser user, IGetEstablishmentIdQuery getEstablishmentIdQuery, IGetSubmissionStatusesQuery getSubmissionStatusesQuery, IGetGroupSelectionQuery getGroupSelectionQuery, IGetSectionQuery getSectionQuery, IGetLatestResponsesQuery getLatestResponsesQuery, IGetSubTopicRecommendationQuery getSubTopicRecommendationQuery, IOptions<ContactOptions> contactOptions, IGetNavigationQuery getNavigationQuery) : base(logger)
        {
            _logger = logger;
            _user = user;
            _getEstablishmentIdQuery = getEstablishmentIdQuery;
            _getSubmissionStatusesQuery = getSubmissionStatusesQuery;
            _getGroupSelectionQuery = getGroupSelectionQuery;
            _getSectionQuery = getSectionQuery;
            _getLatestResponsesQuery = getLatestResponsesQuery;
            _getSubTopicRecommendationQuery = getSubTopicRecommendationQuery;
            _contactOptions = contactOptions;
            _getNavigationQuery = getNavigationQuery;
        }

        [HttpGet($"{GroupsSlug}/{GroupsSelectorPageSlug}")]
        public async Task<IActionResult> GetSelectASchoolView([FromServices] IGetPageQuery getPageQuery, CancellationToken cancellationToken)
        {
            var selectASchoolPageContent = await getPageQuery.GetPageBySlug(GroupsSelectorPageSlug, cancellationToken);
            var dashboardContent = await getPageQuery.GetPageBySlug(GroupsSchoolDashboardSlug, cancellationToken);
            var categories = dashboardContent.Content.OfType<Category>();

            var schools = await _user.GetGroupEstablishments();

            var groupSchools = await GetSchoolsWithSubmissionCounts(schools, categories);

            var groupName = _user.GetOrganisationData().OrgName;
            var title = new Title() { Text = groupName };
            List<ContentComponent> content = selectASchoolPageContent?.Content ?? new List<ContentComponent>();

            string totalSections = "";

            if (categories != null)
            {
                totalSections = SubmissionStatusHelpers.GetTotalSections(categories);
            }

            var contactLink = await _getNavigationQuery.GetLinkById(_contactOptions.Value.LinkId);

            var viewModel = new GroupsSelectorViewModel()
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

            ViewData["Title"] = "Select a school";

            return View(selectASchoolViewName, viewModel);
        }

        [HttpPost($"{GroupsSlug}/{GroupsSelectorPageSlug}")]
        public async Task<IActionResult> SelectSchool(string schoolUrn, string schoolName, [FromServices] IRecordGroupSelectionCommand recordGroupSelectionCommand, CancellationToken cancellationToken = default)
        {
            var dto = new SubmitSelectionDto()
            {
                SelectedEstablishmentUrn = schoolUrn,
                SelectedEstablishmentName = schoolName
            };

            await recordGroupSelectionCommand.RecordGroupSelection(dto, cancellationToken);

            return RedirectToAction("GetSchoolDashboardView");
        }

        [HttpGet($"{GroupsSlug}/{GroupsSchoolDashboardSlug}", Name = GetSchoolDashboardAction)]
        public async Task<IActionResult> GetSchoolDashboardView([FromServices] IGetPageQuery getPageQuery, CancellationToken cancellationToken)
        {
            var latestSelection = await GetCurrentSelection(cancellationToken);

            var groupName = _user.GetOrganisationData().OrgName;
            var pageContent = await getPageQuery.GetPageBySlug(GroupsSchoolDashboardSlug, cancellationToken);
            List<ContentComponent> content = pageContent?.Content ?? new List<ContentComponent>();

            var viewModel = new GroupsSchoolDashboardViewModel()
            {
                SchoolName = latestSelection.SelectedEstablishmentName,
                SchoolId = latestSelection.SelectedEstablishmentId,
                GroupName = groupName,
                Title = new Title() { Text = "Plan technology for your school" },
                Content = content,
                Slug = GroupsSchoolDashboardSlug
            };

            ViewData["Title"] = "Dashboard";

            return View(schoolDashboardViewName, viewModel);
        }

        [HttpGet("{GroupsSlug}/recommendations/{SectionSlug}")]
        public async Task<IActionResult> GetGroupsRecommendation(string sectionSlug, CancellationToken cancellationToken)
        {
            var latestSelection = await GetCurrentSelection(cancellationToken);
            var schoolId = latestSelection.SelectedEstablishmentId;
            var schoolName = latestSelection.SelectedEstablishmentName;

            var section = await _getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken)
                ?? throw new ContentfulDataUnavailableException($"Could not find section for slug: {sectionSlug}");

            var subTopicRecommendation = await _getSubTopicRecommendationQuery.GetSubTopicRecommendation(section.Sys.Id, cancellationToken)
                ?? throw new ContentfulDataUnavailableException($"Could not find subtopic recommendation for section: {section.Name}");
            var submissionResponses = await _getLatestResponsesQuery.GetLatestResponses(schoolId, section.Sys.Id, true, cancellationToken)
                ?? throw new DatabaseException($"Could not find users answers for section: {section.Name}");
            var latestResponses = section.GetOrderedResponsesForJourney(submissionResponses.Responses);

            var customIntro = new GroupsCustomRecommendationIntro()
            {
                HeaderText = $"{section.Name} recommendations",
                IntroContent = "The recommendations are based on the following answers provided by the school when they completed the self-assessment.",
                LinkText = "Overview",
                SelectedEstablishmentName = schoolName,
                Responses = latestResponses.ToList(),
            };

            var subTopicChunks = subTopicRecommendation.Section.GetRecommendationChunksByAnswerIds(latestResponses.Select(answer => answer.AnswerRef));

            var viewModel = new GroupsRecommendationsViewModel
            {
                SectionName = subTopicRecommendation.Subtopic.Name,
                SelectedEstablishmentId = schoolId,
                SelectedEstablishmentName = schoolName,
                Slug = sectionSlug,
                Chunks = subTopicChunks,
                GroupsCustomRecommendationIntro = customIntro,
                SubmissionResponses = latestResponses
            };

            // Passes the school name to the Header
            ViewData["SelectedEstablishmentName"] = viewModel.SelectedEstablishmentName;
            ViewData["Title"] = viewModel.SectionName;

            return View("~/Views/Groups/Recommendations.cshtml", viewModel);
        }

        [HttpGet("groups/recommendations/{sectionSlug}/print", Name = "GetRecommendationsPrintView")]
        public async Task<IActionResult> GetRecommendationsPrintView(int schoolId, string schoolName, string sectionSlug, CancellationToken cancellationToken)
        {
            var section = await _getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken)
                ?? throw new ContentfulDataUnavailableException($"Could not find section for slug: {sectionSlug}");

            var subtopicRecommendation = await _getSubTopicRecommendationQuery.GetSubTopicRecommendation(section.Sys.Id, cancellationToken)
                ?? throw new ContentfulDataUnavailableException($"Could not find subtopic recommendation for section: {section.Name}");
            var submissionResponses = await _getLatestResponsesQuery.GetLatestResponses(schoolId, section.Sys.Id, true, cancellationToken)
                ?? throw new DatabaseException($"Could not get latest responses for school with ID {schoolId} in section: {section.Name}");

            var latestResponses = section.GetOrderedResponsesForJourney(submissionResponses.Responses);
            var customIntro = new GroupsCustomRecommendationIntro()
            {
                HeaderText = $"{section.Name} recommendations",
                IntroContent = "The recommendations are based on the following answers provided by the school when they completed the self-assessment.",
                LinkText = "Overview",
                SelectedEstablishmentName = schoolName,
                Responses = latestResponses.ToList(),
            };

            if (subtopicRecommendation.Section == null)
            {
                return RedirectToAction(GetSchoolDashboardAction);
            }

            var subTopicChunks = subtopicRecommendation.Section.GetRecommendationChunksByAnswerIds(latestResponses.Select(answer => answer.AnswerRef));

            var viewModel = new GroupsRecommendationsViewModel
            {
                SectionName = subtopicRecommendation.Subtopic.Name,
                SelectedEstablishmentId = schoolId,
                SelectedEstablishmentName = schoolName,
                Slug = $"{sectionSlug}/recommendations",
                Chunks = subTopicChunks,
                GroupsCustomRecommendationIntro = customIntro,
                SubmissionResponses = latestResponses
            };

            ViewData["Title"] = viewModel.SectionName;

            return View("~/Views/Groups/RecommendationsChecklist.cshtml", viewModel);
        }

        [NonAction]
        public async Task<GroupReadActivityDto> GetCurrentSelection(CancellationToken cancellationToken)
        {
            var userId = await _user.GetCurrentUserId();
            var userEstablishmentId = await _user.GetEstablishmentId();
            var latestSelection = await _getGroupSelectionQuery.GetLatestSelectedGroupSchool(userId.Value, userEstablishmentId, cancellationToken)
                ?? throw new DatabaseException($"Could not get latest selected group school for user with ID {userId.Value} in establishment: {userEstablishmentId}");

            return latestSelection;
        }

        [NonAction]
        public async Task<List<EstablishmentLink>> GetSchoolsWithSubmissionCounts(List<EstablishmentLink> schools, IEnumerable<Category> categories)
        {
            foreach (var school in schools)
            {
                var establishmentId = await _getEstablishmentIdQuery.GetEstablishmentId(school.Urn);
                var completedSectionsCount = 0;
                foreach (var category in categories)
                {
                    var categoryWithStatus = await SubmissionStatusHelpers.RetrieveSectionStatuses(category, _logger, _getSubmissionStatusesQuery, establishmentId);
                    completedSectionsCount += categoryWithStatus.Completed;
                }
                school.CompletedSectionsCount = completedSectionsCount;
            }
            return schools;
        }
    }
}
