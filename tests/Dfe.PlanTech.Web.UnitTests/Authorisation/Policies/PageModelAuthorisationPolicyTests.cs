using System.Security.Claims;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Infrastructure.SignIn.Models;
using Dfe.PlanTech.Web.Authorisation.Policies;
using Dfe.PlanTech.Web.Authorisation.Requirements;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Authorisation.Policies;

using System.Security.Claims;
using System.Threading.Tasks;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Infrastructure.SignIn.Models;
using Dfe.PlanTech.Web.Authorisation.Policies;
using Dfe.PlanTech.Web.Authorisation.Requirements;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

public class PageModelAuthorisationPolicyTests
{
    private static (HttpContext http, IContentfulService contentful) HttpWithServices()
    {
        var services = new ServiceCollection();
        var contentful = Substitute.For<IContentfulService>();
        services.AddScoped(_ => contentful);

        var sp = services.BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = sp };

        // Seed required route keys to avoid KeyNotFound noise in logs
        http.Request.RouteValues[PageModelAuthorisationPolicy.RouteValuesControllerNameKey] = PagesController.ControllerName;
        http.Request.RouteValues[PageModelAuthorisationPolicy.RouteValuesActionNameKey] = "Index";

        return (http, contentful);
    }

    private static AuthorizationHandlerContext Ctx(HttpContext http)
    {
        // Build principal with no auth by default
        var principal = new ClaimsPrincipal(new ClaimsIdentity()); // IsAuthenticated = false
        var requirement = new PageAuthorisationRequirement();
        return new AuthorizationHandlerContext(new[] { requirement }, principal, http);
    }

    private static AuthorizationHandlerContext Ctx(HttpContext http, bool authenticated)
    {
        var id = new ClaimsIdentity();
        if (authenticated)
        {
            id = new ClaimsIdentity(authenticationType: "test"); // authenticated
        }
        var principal = new ClaimsPrincipal(id);
        var requirement = new PageAuthorisationRequirement();
        return new AuthorizationHandlerContext(new[] { requirement }, principal, http);
    }

    private static PageModelAuthorisationPolicy SUT(out ILogger<PageModelAuthorisationPolicy> logger)
    {
        logger = Substitute.For<ILogger<PageModelAuthorisationPolicy>>();
        return new PageModelAuthorisationPolicy(logger);
    }

    [Fact]
    public async Task When_Resource_Is_Not_HttpContext_Logs_Error_And_Does_Not_Succeed_Or_Fail()
    {
        var handler = SUT(out var logger);
        var ctx = new AuthorizationHandlerContext(new[] { new PageAuthorisationRequirement() }, new ClaimsPrincipal(), resource: new object());

        await handler.HandleAsync(ctx);

        Assert.False(ctx.HasSucceeded);
        Assert.False(ctx.HasFailed);
        logger.ReceivedWithAnyArgs(1).Log(default, default, default!, default, default!);
    }

    [Fact]
    public async Task Non_Pages_Controller_Requires_Auth_Succeeds_When_User_Authenticated()
    {
        var (http, _) = HttpWithServices();
        http.Request.RouteValues[PageModelAuthorisationPolicy.RouteValuesControllerNameKey] = "OtherController"; // not Pages
        var ctx = Ctx(http, authenticated: true);

        var handler = SUT(out _);
        await handler.HandleAsync(ctx);

        Assert.True(ctx.HasSucceeded);
        Assert.False(ctx.HasFailed);

        // UserAuthorisationResult is stored
        Assert.True(http.Items.ContainsKey(UserAuthorisationResult.HttpContextKey));
    }

    [Fact]
    public async Task Non_Pages_Controller_Requires_Auth_Fails_When_User_Not_Authenticated()
    {
        var (http, _) = HttpWithServices();
        http.Request.RouteValues[PageModelAuthorisationPolicy.RouteValuesControllerNameKey] = "OtherController"; // not Pages
        var ctx = Ctx(http, authenticated: false);

        var handler = SUT(out _);
        await handler.HandleAsync(ctx);

        Assert.False(ctx.HasSucceeded);
        Assert.True(ctx.HasFailed);
        Assert.True(http.Items.ContainsKey(UserAuthorisationResult.HttpContextKey));
    }

    [Fact]
    public async Task Pages_Controller_Page_Does_Not_Require_Auth_Succeeds()
    {
        var (http, contentful) = HttpWithServices();
        http.Request.RouteValues[PageModelAuthorisationPolicy.RoutesValuesRouteNameKey] = "/welcome";
        var page = new PageEntry { RequiresAuthorisation = false };
        contentful.GetPageBySlugAsync("/welcome").Returns(page);

        var ctx = Ctx(http, authenticated: false); // even unauthenticated should pass
        var handler = SUT(out _);

        await handler.HandleAsync(ctx);

        Assert.True(ctx.HasSucceeded);
        Assert.False(ctx.HasFailed);
        Assert.Equal(page, http.Items[nameof(PageEntry)]);
    }

    [Fact]
    public async Task Pages_Controller_Page_Requires_Auth_Succeeds_When_User_Authenticated()
    {
        var (http, contentful) = HttpWithServices();
        http.Request.RouteValues[PageModelAuthorisationPolicy.RoutesValuesRouteNameKey] = "/secure";
        var page = new PageEntry { RequiresAuthorisation = true };
        contentful.GetPageBySlugAsync("/secure").Returns(page);

        var ctx = Ctx(http, authenticated: true);
        var handler = SUT(out _);

        await handler.HandleAsync(ctx);

        Assert.True(ctx.HasSucceeded);
        Assert.False(ctx.HasFailed);
        Assert.Equal(page, http.Items[nameof(PageEntry)]);
    }

    [Fact]
    public async Task Pages_Controller_Page_Requires_Auth_Fails_When_User_Not_Authenticated()
    {
        var (http, contentful) = HttpWithServices();
        http.Request.RouteValues[PageModelAuthorisationPolicy.RoutesValuesRouteNameKey] = "/secure";
        var page = new PageEntry { RequiresAuthorisation = true };
        contentful.GetPageBySlugAsync("/secure").Returns(page);

        var ctx = Ctx(http, authenticated: false);
        var handler = SUT(out _);

        await handler.HandleAsync(ctx);

        Assert.False(ctx.HasSucceeded);
        Assert.True(ctx.HasFailed);
        Assert.Equal(page, http.Items[nameof(PageEntry)]);
    }

    [Fact]
    public async Task Pages_Controller_Missing_Slug_Treated_As_Index_Calls_Service_With_Slash_And_Succeeds_When_Page_Missing()
    {
        var (http, contentful) = HttpWithServices();
        http.Request.RouteValues.Remove(PageModelAuthorisationPolicy.RoutesValuesRouteNameKey); // missing slug

        // Service returns null -> treated as "no auth required" in policy (so succeeds)
        contentful.GetPageBySlugAsync("/").Returns((PageEntry?)null);

        var ctx = Ctx(http, authenticated: false);
        var handler = SUT(out _);

        await handler.HandleAsync(ctx);

        Assert.True(ctx.HasSucceeded);
        Assert.False(ctx.HasFailed);

        // Route was normalised to "/"
        Assert.Equal("/", http.Request.RouteValues[PageModelAuthorisationPolicy.RoutesValuesRouteNameKey]);

        // PageEntry key exists (value is null but key should be present)
        Assert.True(http.Items.ContainsKey(nameof(PageEntry)));
        Assert.Null(http.Items[nameof(PageEntry)]);
    }

    [Fact]
    public async Task Always_Stores_UserAuthorisationResult_In_HttpContext_Items()
    {
        var (http, contentful) = HttpWithServices();
        http.Request.RouteValues[PageModelAuthorisationPolicy.RoutesValuesRouteNameKey] = "/any";
        contentful.GetPageBySlugAsync("/any").Returns(new PageEntry { RequiresAuthorisation = false });

        var ctx = Ctx(http, authenticated: false);
        var handler = SUT(out _);

        await handler.HandleAsync(ctx);

        Assert.True(http.Items.ContainsKey(UserAuthorisationResult.HttpContextKey));
        Assert.IsType<UserAuthorisationResult>(http.Items[UserAuthorisationResult.HttpContextKey]);
    }
}
