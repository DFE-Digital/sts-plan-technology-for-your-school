using Dfe.PlanTech.Web.Attributes;
using Dfe.PlanTech.Web.Context.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Attributes;

public class ValidateMatSelectedAttributeTests
{
    [Fact]
    public void OnActionExecuting_WhenAuthenticatedMatAndNoSchoolSelected_RedirectsToSelectSchool()
    {
        var currentUser = CreateCurrentUser(true, true, null);
        var context = CreateActionExecutingContext(currentUser);

        new ValidateMatSelectedAttribute().OnActionExecuting(context);

        var result = Assert.IsType<RedirectToRouteResult>(context.Result);

        Assert.Equal("Groups", result.RouteValues["controller"]);
        Assert.Equal("GetSelectASchoolView", result.RouteValues["action"]);
    }

    [Theory]
    [InlineData(true, true, "123456")]
    [InlineData(true, false, null)]
    [InlineData(false, true, null)]
    [InlineData(false, false, null)]
    public void OnActionExecuting_WhenRedirectConditionsAreNotMet_DoesNotRedirect(
        bool isAuthenticated,
        bool isMat,
        string? groupSelectedSchoolUrn)
    {
        var currentUser = CreateCurrentUser(isAuthenticated, isMat, groupSelectedSchoolUrn);
        var context = CreateActionExecutingContext(currentUser);

        new ValidateMatSelectedAttribute().OnActionExecuting(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public void OnActionExecuting_SetsRedirectResult_WithCorrectRouteValues()
    {
        // arrange
        var currentUser = CreateCurrentUser(
            isAuthenticated: true,
            isMat: true,
            groupSelectedSchoolUrn: null
        );

        var context = CreateActionExecutingContext(currentUser);
        var attribute = new ValidateMatSelectedAttribute();

        // act
        attribute.OnActionExecuting(context);

        // assert
        Assert.NotNull(context.Result);

        var redirect = Assert.IsType<RedirectToRouteResult>(context.Result);

        Assert.Equal("Groups", redirect.RouteValues["controller"]);
        Assert.Equal("GetSelectASchoolView", redirect.RouteValues["action"]);
    }

    private static ICurrentUser CreateCurrentUser(
        bool isAuthenticated,
        bool isMat,
        string? groupSelectedSchoolUrn)
    {
        var currentUser = Substitute.For<ICurrentUser>();

        currentUser.IsAuthenticated.Returns(isAuthenticated);
        currentUser.IsMat.Returns(isMat);
        currentUser.GroupSelectedSchoolUrn.Returns(groupSelectedSchoolUrn);

        return currentUser;
    }

    private static ActionExecutingContext CreateActionExecutingContext(ICurrentUser currentUser)
    {
        var serviceProvider = Substitute.For<IServiceProvider>();

        serviceProvider
            .GetService(typeof(ICurrentUser))
            .Returns(currentUser);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor(),
            new ModelStateDictionary()
        );

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object()
        );
    }
}
