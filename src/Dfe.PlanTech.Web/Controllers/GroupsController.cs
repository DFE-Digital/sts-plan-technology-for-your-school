using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Groups.Interfaces;
using Dfe.PlanTech.Domain.Groups.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

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

        public GroupsController(ILogger<GroupsController> logger, IUser user, IGetEstablishmentIdQuery getEstablishmentIdQuery, IGetSubmissionStatusesQuery getSubmissionStatusesQuery, IGetGroupSelectionQuery getGroupSelectionQuery, IGetSectionQuery getSectionQuery, IGetLatestResponsesQuery getLatestResponsesQuery, IGetSubTopicRecommendationQuery getSubTopicRecommendationQuery) : base(logger)
        {
            _logger = logger;
            _user = user;
            _getEstablishmentIdQuery = getEstablishmentIdQuery;
            _getSubmissionStatusesQuery = getSubmissionStatusesQuery;
            _getGroupSelectionQuery = getGroupSelectionQuery;
            _getSectionQuery = getSectionQuery;
            _getLatestResponsesQuery = getLatestResponsesQuery;
            _getSubTopicRecommendationQuery = getSubTopicRecommendationQuery;
        }

        [HttpGet($"{GroupsSlug}/{GroupsSelectorPageSlug}")]
        public async Task<IActionResult> GetSelectASchoolView([FromServices] IGetPageQuery getPageQuery, CancellationToken cancellationToken)
        {
            var selectASchoolPageContent = await getPageQuery.GetPageBySlug(GroupsSelectorPageSlug, cancellationToken);

            var schools = await _user.GetGroupEstablishments();
            var groupName = _user.GetOrganisationData().OrgName;
            var title = new Title() { Text = groupName };
            List<ContentComponent> content = selectASchoolPageContent?.Content ?? new List<ContentComponent>();

            var viewModel = new GroupsSelectorViewModel()
            {
                GroupName = groupName,
                GroupEstablishments = schools,
                Title = title,
                Content = content
            };

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

            return View(schoolDashboardViewName, viewModel);
        }

        [HttpGet("{GroupsSlug}/recommendations/{SectionSlug}")]
        public async Task<IActionResult> GetGroupsRecommendation(string sectionSlug, CancellationToken cancellationToken)
        {
            var latestSelection = await GetCurrentSelection(cancellationToken);
            var schoolId = latestSelection.SelectedEstablishmentId;
            var schoolName = latestSelection.SelectedEstablishmentName;

            var section = await _getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken);

            var subTopicRecommendation = await _getSubTopicRecommendationQuery.GetSubTopicRecommendation(section.Sys.Id, cancellationToken) ?? throw new ContentfulDataUnavailableException($"Could not find subtopic recommendation for:  {section.Name}");
            var submissionResponses = await _getLatestResponsesQuery.GetLatestResponses(schoolId, section.Sys.Id, true, cancellationToken) ?? throw new DatabaseException($"Could not find users answers for:  {section.Name}");
            var latestResponses = section.GetOrderedResponsesForJourney(submissionResponses.Responses);

            var customIntro = new GroupsCustomRecommendationIntro()
            {
                HeaderText = "Overview",
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
                Slug = $"{sectionSlug}/recommendations",
                Chunks = subTopicChunks,
                GroupsCustomRecommendationIntro = customIntro,
                SubmissionResponses = latestResponses
            };
            return View("~/Views/Groups/Recommendations.cshtml", viewModel);
        }

        public IActionResult GetRecommendationsPrintView(int schoolId, string schoolName, SubtopicRecommendation subtopicRecommendation, string sectionSlug, IEnumerable<QuestionWithAnswer> latestResponses)
        {
            var customIntro = new GroupsCustomRecommendationIntro()
            {
                HeaderText = "Overview",
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

            return View("~/Views/Recommendations/RecommendationsChecklist.cshtml", viewModel);
        }

        [NonAction]
        public async Task<GroupReadActivityDto> GetCurrentSelection(CancellationToken cancellationToken)
        {
            var userId = await _user.GetCurrentUserId();
            var userEstablishmentId = await _user.GetEstablishmentId();
            var latestSelection = await _getGroupSelectionQuery.GetLatestSelectedGroupSchool(userId.Value, userEstablishmentId, cancellationToken);

            return latestSelection;
        }
    }
}
