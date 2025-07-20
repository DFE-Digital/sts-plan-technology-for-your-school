using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Core.Constants;
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
        public const string GetGroupsRecommendationAction = "GetGroupsRecommendation";
        public const string GetSchoolDashboardAction = "GetSchoolDashboard";
        private const string SelectASchoolViewName = "~/Views/Groups/GroupsSelectSchool.cshtml";
        private const string schoolDashboardViewName = "~/Views/Groups/GroupsSchoolDashboard.cshtml";

        private readonly ILogger _logger;
        private readonly CurrentUser _currentUser;
        private ContactOptionsConfiguration _contactOptions;
        private GroupsViewBuilder _groupsViewBuilder;
        private GroupService _groupService;

        public GroupsController(
            ILogger<GroupsController> logger,
            IOptions<ContactOptionsConfiguration> contactOptions,
            CurrentUser currentUser,
            GroupsViewBuilder groupsViewBuilder,
            GroupService groupService
        ) : base(logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _contactOptions = contactOptions?.Value ?? throw new ArgumentNullException(nameof(contactOptions));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _groupsViewBuilder = groupsViewBuilder ?? throw new ArgumentNullException(nameof(groupsViewBuilder));
            _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
        }

        [HttpGet($"{UrlConstants.GroupsSlug}/{UrlConstants.GroupsSelectionPageSlug}")]
        public async Task<IActionResult> GetSelectASchoolView()
        {
            var viewModel = await _groupsViewBuilder.GetSelectASchoolViewModelAsync(this);

            ViewData["Title"] = "Select a school";
            return View(SelectASchoolViewName, viewModel);
        }

        [HttpPost($"{UrlConstants.GroupsSlug}/{UrlConstants.GroupsSelectionPageSlug}")]
        public async Task<IActionResult> SelectSchool(string schoolUrn, string schoolName)
        {
            await _groupsViewBuilder.RecordGroupSelectionAsync(schoolUrn, schoolName);

            return RedirectToAction("GetSchoolDashboardView");
        }

        [HttpGet($"{UrlConstants.GroupsSlug}/{UrlConstants.GroupsDashboardSlug}", Name = GetSchoolDashboardAction)]
        public async Task<IActionResult> GetSchoolDashboardView()
        {
            var viewModel = await _groupsViewBuilder.GetSchoolDashboardViewAsync();

            ViewData["Title"] = "Dashboard";
            return View(schoolDashboardViewName, viewModel);
        }

        [HttpGet($"{UrlConstants.GroupsSlug}/recommendations/{{SectionSlug}}")]
        public async Task<IActionResult> GetGroupsRecommendation(string sectionSlug)
        {
            var viewModel = await _groupsViewBuilder.GetGroupsRecommendationAsync(sectionSlug);

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
    }
}
