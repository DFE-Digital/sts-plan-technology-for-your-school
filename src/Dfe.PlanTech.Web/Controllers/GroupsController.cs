using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;
using Dfe.PlanTech.Domain.Establishments.Models;
using Dfe.PlanTech.Domain.Groups.Interfaces;
using Dfe.PlanTech.Domain.Groups.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Configurations;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.ViewBuilders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.Controllers
{
    [Route("/")]
    public class GroupsController : BaseController<GroupsController>
    {
        public const string GetSchoolDashboardAction = "GetSchoolDashboard";
        private const string schoolDashboardViewName = "~/Views/Groups/GroupsSchoolDashboard.cshtml";
        public const string GetGroupsRecommendationAction = "GetGroupsRecommendation";

        private readonly ILogger _logger;
        private readonly CurrentUser _currentUser;
        private ContactOptionsConfiguration _contactOptions;
        private GroupsViewBuilder _groupsViewBuilder;

        public GroupsController(
            ILogger<GroupsController> logger,
            IOptions<ContactOptionsConfiguration> contactOptions,
            CurrentUser currentUser,
            GroupsViewBuilder groupsViewBuilder
        ) : base(logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _contactOptions = contactOptions?.Value ?? throw new ArgumentNullException(nameof(contactOptions));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _groupsViewBuilder = groupsViewBuilder ?? throw new ArgumentNullException(nameof(groupsViewBuilder));
        }

        [HttpGet($"{UrlConstants.GroupsSlug}/{UrlConstants.GroupsSelectionPageSlug}")]
        public async Task<IActionResult> GetSelectASchoolView()
        {
            return await _groupsViewBuilder.GetSelectASchoolView(this);
        }

        [HttpPost($"{GroupsSlug}/{UrlConstants.GroupsSelectionPageSlug}")]
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

        [HttpGet($"{GroupsSlug}/{UrlConstants.GroupsDashboardSlug}", Name = GetSchoolDashboardAction)]
        public async Task<IActionResult> GetSchoolDashboardView([FromServices] IGetPageQuery getPageQuery, CancellationToken cancellationToken)
        {
            var latestSelection = await GetCurrentSelection(cancellationToken);

            var groupName = _currentUser.GetOrganisationData().OrgName;
            var pageContent = await getPageQuery.GetPageBySlug(UrlConstants.GroupsDashboardSlug, cancellationToken);
            List<ContentComponent> content = pageContent?.Content ?? new List<ContentComponent>();

            var viewModel = new GroupsSchoolDashboardViewModel()
            {
                SchoolName = latestSelection.SelectedEstablishmentName,
                SchoolId = latestSelection.SelectedEstablishmentId,
                GroupName = groupName,
                Title = new Title() { Text = "Plan technology for your school" },
                Content = content,
                Slug = UrlConstants.GroupsDashboardSlug
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

            var subTopicChunks = subTopicRecommendation.Section.GetRecommendationChunksByAnswerIds(latestResponses.Select(answer => answer.AnswerSysId));

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

            var subTopicChunks = subtopicRecommendation.Section.GetRecommendationChunksByAnswerIds(latestResponses.Select(answer => answer.AnswerSysId));

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
            var userId = await _currentUser.GetCurrentUserId();
            var userEstablishmentId = await _currentUser.GetEstablishmentId();
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
                    var categoryWithStatus = await SubmissionStatusHelper.RetrieveSectionStatuses(category, _logger, _getSubmissionStatusesQuery, establishmentId);
                    completedSectionsCount += categoryWithStatus.Completed;

                    foreach (var sectionStatus in categoryWithStatus.SectionStatuses)
                    {
                        if (sectionStatus.Completed == false && sectionStatus.LastCompletionDate is not null)
                        {
                            completedSectionsCount++;
                        }
                    }
                }
                school.CompletedSectionsCount = completedSectionsCount;
            }
            return schools;
        }
    }
}
