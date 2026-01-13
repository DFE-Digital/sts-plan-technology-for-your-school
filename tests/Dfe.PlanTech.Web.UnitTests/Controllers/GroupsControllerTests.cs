using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class GroupsControllerTests
    {
        private readonly ILogger<GroupsController> _logger;
        private readonly IGroupsViewBuilder _viewBuilder;
        private readonly ICurrentUser _currentUser;
        private readonly GroupsController _controller;

        public GroupsControllerTests()
        {
            _logger = Substitute.For<ILogger<GroupsController>>();
            _viewBuilder = Substitute.For<IGroupsViewBuilder>();
            _currentUser = Substitute.For<ICurrentUser>();
            _controller = new GroupsController(_logger, _currentUser, _viewBuilder);
        }

        [Fact]
        public void Constructor_WithNullCurrentUser_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new GroupsController(_logger, null!, _viewBuilder)
            );

            Assert.Equal("currentUser", ex.ParamName);
        }

        [Fact]
        public void Constructor_WithNullViewBuilder_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new GroupsController(_logger, _currentUser, null!)
            );

            Assert.Equal("groupsViewBuilder", ex.ParamName);
        }

        [Fact]
        public async Task GetSelectASchoolView_CallsViewBuilderAndReturnsResult()
        {
            _viewBuilder.RouteToSelectASchoolViewModelAsync(_controller)
                .Returns(new OkResult());

            var result = await _controller.GetSelectASchoolView();

            await _viewBuilder.Received(1).RouteToSelectASchoolViewModelAsync(_controller);
            Assert.IsType<OkResult>(result);
        }


        [Fact]
        public async Task SelectSchool_RecordsSelectionAndRedirects()
        {
            var schoolUrn = "123456";
            var schoolName = "Test School";

            var controller = new GroupsController(_logger, _currentUser, _viewBuilder);

            var result = await controller.SelectSchool(schoolUrn, schoolName);

            await _viewBuilder.Received(1).RecordGroupSelectionAsync(schoolUrn, schoolName);
            _currentUser.Received(1).SetGroupSelectedSchool(schoolUrn, schoolName);

            var redirect = Assert.IsType<RedirectResult>(result);
            Assert.Equal(UrlConstants.HomePage, redirect.Url);
        }
    }
}
