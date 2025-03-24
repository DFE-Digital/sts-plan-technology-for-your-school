using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Establishments.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Middleware;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.Routing;
using Dfe.PlanTech.Web.UnitTests.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Routing;

public class GroupsRouterTests
{
    private readonly IGroupsRouter _groupsRouter;
    private readonly GroupsController _groupsController;
    private readonly IComponentBuilder _componentBuilder;
    private readonly ILogger<GroupsController> _logger = Substitute.For<ILogger<GroupsController>>();
    private readonly IExceptionHandlerMiddleware _exceptionHandler = Substitute.For<IExceptionHandlerMiddleware>();
    private readonly IUser _user = Substitute.For<IUser>();

    public GroupsRouterTests()
    {
        _groupsRouter = new GroupsRouter();
        _groupsController = new GroupsController(_logger, _exceptionHandler, _user, _groupsRouter);
        _componentBuilder = new ComponentBuilder();
    }

    [Fact]
    public void GetSelectASchool_Returns_ViewResult_With_Correct_ViewModel()
    {
        var groupName = "Test Group";
        var schools = new List<EstablishmentLink> { new EstablishmentLink { Id = 1, GroupUid = "123" ,EstablishmentName = "School 1" } };
        Title title = new Title { Text = "Testing Title" };
        var content = new List<ContentComponent> { _componentBuilder.BuildTextBody() };

        var result = _groupsRouter.GetSelectASchool(groupName, schools, title, content, _groupsController, CancellationToken.None);

        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<GroupsSelectorViewModel>(viewResult.Model);
        Assert.Equal(groupName, viewModel.GroupName);
        Assert.Equal(schools, viewModel.GroupEstablishments);
        Assert.Equal(title, viewModel.Title);
        Assert.Equal(content, viewModel.Content);
    }

    [Fact]
    public void GetSchoolDashboard_Returns_ViewResult_With_Correct_ViewModel()
    {
        var schoolId = "1";
        var schoolName = "School 1";
        var groupName = "Test Group";
        var content = new List<ContentComponent> { _componentBuilder.BuildTextBody() };

        var result = _groupsRouter.GetSchoolDashboard(schoolId, schoolName, groupName, content, _groupsController, CancellationToken.None);

        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<GroupsSchoolDashboardViewModel>(viewResult.Model);
        Assert.Equal(schoolId, viewModel.SchoolId);
        Assert.Equal(schoolName, viewModel.SchoolName);
        Assert.Equal(groupName, viewModel.GroupName);
        Assert.Equal(content, viewModel.Content);
    }
}
