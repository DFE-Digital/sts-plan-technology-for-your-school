using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Establishments.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Middleware;
using Dfe.PlanTech.Web.Routing;
using Dfe.PlanTech.Web.UnitTests.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class GroupsControllerTests
    {
        private readonly ILogger<GroupsController> _logger;
        private readonly IExceptionHandlerMiddleware _exceptionHandler;
        private readonly IUser _user;
        private readonly IGroupsRouter _groupsRouter;
        private readonly IGetPageQuery _getPageQuery;
        private readonly IComponentBuilder _componentBuilder;

        public GroupsControllerTests()
        {
            _logger = Substitute.For<ILogger<GroupsController>>();
            _exceptionHandler = Substitute.For<IExceptionHandlerMiddleware>();
            _user = Substitute.For<IUser>();
            _groupsRouter = Substitute.For<IGroupsRouter>();
            _getPageQuery = Substitute.For<IGetPageQuery>();
            _componentBuilder = Substitute.For<IComponentBuilder>();
        }

        [Fact]
        public async Task GetSelectASchoolView_Returns_Correct_View()
        {
            var controller = new GroupsController(_logger, _exceptionHandler, _user, _groupsRouter);

            var pageContent = _componentBuilder.BuildPage();

            _getPageQuery.GetPageBySlug("testing-page", Arg.Any<CancellationToken>()).Returns(pageContent);
            _user.GetGroupEstablishments().Returns(new List<EstablishmentLink> { new EstablishmentLink { Id = 1, EstablishmentName = "School 1" } });
            _user.GetOrganisationData().Returns(new EstablishmentDto { OrgName = "Test Group" });

            var result = await controller.GetSelectASchoolView(_getPageQuery, CancellationToken.None);

            _groupsRouter.Received(1).GetSelectASchool("Test Group", Arg.Any<List<EstablishmentLink>>(), Arg.Any<Title>(), Arg.Any<List<ContentComponent>>(), controller, CancellationToken.None);
        }

        [Fact]
        public void SelectSchool_Sets_TempData_And_Redirects()
        {
            var controller = new GroupsController(_logger, _exceptionHandler, _user, _groupsRouter);
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Substitute.For<ITempDataProvider>());
            controller.TempData = tempData;

            var schoolId = "1";
            var schoolName = "School 1";

            var result = controller.SelectSchool(schoolId, schoolName);

            Assert.Equal(schoolId, controller.TempData["SelectedSchoolId"]);
            Assert.Equal(schoolName, controller.TempData["SelectedSchoolName"]);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("GetSchoolDashboardView", redirectResult.ActionName);
        }
        
        [Fact]
        public async Task GetSchoolDashboardView_Returns_Correct_View()
        {
            var controller = new GroupsController(_logger, _exceptionHandler, _user, _groupsRouter);
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Substitute.For<ITempDataProvider>());
            controller.TempData = tempData;

            controller.TempData["SelectedSchoolId"] = "1";
            controller.TempData["SelectedSchoolName"] = "School 1";
            _user.GetOrganisationData().Returns(new EstablishmentDto { OrgName = "Test Group" });

            var pageContent = _componentBuilder.BuildPage();
            _getPageQuery.GetPageBySlug("testing-page", Arg.Any<CancellationToken>()).Returns(pageContent);

            var result = await controller.GetSchoolDashboardView(_getPageQuery, CancellationToken.None);

            _groupsRouter.Received(1).GetSchoolDashboard("1", "School 1", "Test Group", Arg.Any<List<ContentComponent>>(), controller, CancellationToken.None);
        }
    }
}
