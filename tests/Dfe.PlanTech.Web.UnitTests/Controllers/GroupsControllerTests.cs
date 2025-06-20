using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Establishments.Models;
using Dfe.PlanTech.Domain.Groups.Interfaces;
using Dfe.PlanTech.Domain.Groups.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Configurations;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class GroupsControllerTests
    {
        private readonly IUser _user;
        private readonly IGetGroupSelectionQuery _getGroupSelectionQuery;
        private readonly IGetPageQuery _getPageQuery;
        private readonly IGetSectionQuery _getSectionQuery;
        private readonly IGetSubTopicRecommendationQuery _getSubTopicRecommendationQuery;
        private readonly IOptions<ContactOptionsConfiguration> _contactOptions;
        private readonly IGetNavigationQuery _getNavigationQuery;

        private readonly GroupsController _controller;

        public GroupsControllerTests()
        {
            var logger = Substitute.For<ILogger<GroupsController>>();
            _user = Substitute.For<IUser>();
            _getGroupSelectionQuery = Substitute.For<IGetGroupSelectionQuery>();
            _getPageQuery = Substitute.For<IGetPageQuery>();
            _getSectionQuery = Substitute.For<IGetSectionQuery>();
            _getSubTopicRecommendationQuery = Substitute.For<IGetSubTopicRecommendationQuery>();

            _controller = new GroupsController(
                logger,
                _user,
                Substitute.For<IGetEstablishmentIdQuery>(),
                Substitute.For<IGetSubmissionStatusesQuery>(),
                _getGroupSelectionQuery,
                Substitute.For<IGetSectionQuery>(),
                Substitute.For<IGetLatestResponsesQuery>(),
                Substitute.For<IGetSubTopicRecommendationQuery>()
            );
        }

        [Fact]
        public async Task GetSchoolDashboardView_ReturnsViewWithViewModel()
        {
            var selection = new GroupReadActivityDto
            {
                SelectedEstablishmentId = 101,
                SelectedEstablishmentName = "Test School"
            };
            var orgName = "Group Org";

            _user.GetCurrentUserId().Returns(Task.FromResult<int?>(1));
            _user.GetEstablishmentId().Returns(Task.FromResult(100));
            _user.GetOrganisationData().Returns(new EstablishmentDto() { OrgName = orgName });

            _getGroupSelectionQuery
                .GetLatestSelectedGroupSchool(1, 100, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<GroupReadActivityDto?>(selection));

            _getPageQuery.GetPageBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>())!
                .Returns(Task.FromResult(new Page { Content = new List<ContentComponent>() }));

            var result = await _controller.GetSchoolDashboardView(_getPageQuery, CancellationToken.None);

            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel = Assert.IsType<GroupsSchoolDashboardViewModel>(viewResult.Model);

            Assert.Equal("Test School", viewModel.SchoolName);
            Assert.Equal(101, viewModel.SchoolId);
            Assert.Equal(orgName, viewModel.GroupName);
        }

        [Fact]
        public async Task SelectSchool_PostRedirectsToDashboard()
        {
            var recordGroupSelectionCommand = Substitute.For<IRecordGroupSelectionCommand>();

            var result = await _controller.SelectSchool("12345", "School A", recordGroupSelectionCommand,
                CancellationToken.None);

            await recordGroupSelectionCommand.Received(1)
                .RecordGroupSelection(Arg.Is<SubmitSelectionDto>(dto =>
                        dto.SelectedEstablishmentUrn == "12345" &&
                        dto.SelectedEstablishmentName == "School A"),
                    Arg.Any<CancellationToken>());

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("GetSchoolDashboardView", redirect.ActionName);
        }

        [Fact]
        public async Task GetSelectASchoolView_ReturnsViewWithCorrectModel()
        {
            var getNavigationQuery = Substitute.For<IGetNavigationQuery>();
            var contactOptions = Substitute.For<IOptions<ContactOptionsConfiguration>>();
            var mockSchools = new List<EstablishmentLink>
                { new EstablishmentLink() { Urn = "123", EstablishmentName = "School A" } };
            var orgData = new EstablishmentDto { OrgName = "GroupName" };

            _user.GetGroupEstablishments().Returns(mockSchools);
            _user.GetOrganisationData().Returns(orgData);

            _getPageQuery.GetPageBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new Page { Content = new List<ContentComponent>() });

            var contactLinkHref = "contactLinkHref";
            var contactLink = Substitute.For<INavigationLink>();
            contactLink.Href = contactLinkHref;

            var options = new ContactOptionsConfiguration
            {
                LinkId = contactLinkHref
            };

            contactOptions.Value.Returns(options);
            getNavigationQuery.GetLinkById(contactLinkHref).Returns(contactLink);

            var result = await _controller.GetSelectASchoolView(_getPageQuery, getNavigationQuery, contactOptions, CancellationToken.None);

            var view = Assert.IsType<ViewResult>(result);
            var viewModel = Assert.IsType<GroupsSelectorViewModel>(view.Model);

            Assert.Equal("GroupName", viewModel.GroupName);
            Assert.Single(viewModel.GroupEstablishments);
            Assert.Equal("School A", viewModel.GroupEstablishments.First().EstablishmentName);
        }
        [Fact]
        public async Task GetCurrentSelection_ReturnsLatestSelection()
        {
            var user = Substitute.For<IUser>();
            var getGroupSelectionQuery = Substitute.For<IGetGroupSelectionQuery>();
            var logger = Substitute.For<ILogger<GroupsController>>();

            var controller = new GroupsController(
                logger,
                user,
                Substitute.For<IGetEstablishmentIdQuery>(),
                Substitute.For<IGetSubmissionStatusesQuery>(),
                getGroupSelectionQuery,
                Substitute.For<IGetSectionQuery>(),
                Substitute.For<IGetLatestResponsesQuery>(),
                Substitute.For<IGetSubTopicRecommendationQuery>()
            );

            var cancellationToken = CancellationToken.None;
            var expectedSelection = new GroupReadActivityDto { SelectedEstablishmentId = 123, SelectedEstablishmentName = "Test School" };

            user.GetCurrentUserId().Returns(101);
            user.GetEstablishmentId().Returns(456);
            getGroupSelectionQuery.GetLatestSelectedGroupSchool(Arg.Any<int>(), Arg.Any<int>(), cancellationToken)
                .Returns(expectedSelection);

            var result = await controller.GetCurrentSelection(cancellationToken);

            Assert.Equal(expectedSelection, result);
        }

    }
}
