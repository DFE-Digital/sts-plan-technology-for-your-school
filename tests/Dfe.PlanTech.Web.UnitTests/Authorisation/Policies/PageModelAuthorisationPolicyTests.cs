using System.Security.Claims;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.UnitTests.Shared.Extensions;
using Dfe.PlanTech.Web.Authorisation.Policies;
using Dfe.PlanTech.Web.Authorisation.Requirements;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Web.UnitTests.Authorisation.Policies;

public class PageModelAuthorisationPolicyTests
{
    private readonly IContentfulService _contentfulService;
    private readonly PageModelAuthorisationPolicy _policy;
    private AuthorizationHandlerContext _authContext;
    private readonly ILogger<PageModelAuthorisationPolicy> _logger;
    private readonly HttpContext _httpContext;

    private static PageEntry AuthNotRequiredPage = new PageEntry { RequiresAuthorisation = false };
    private static PageEntry AuthRequiredPage = new PageEntry { RequiresAuthorisation = true };

    public PageModelAuthorisationPolicyTests()
    {
        _logger = Substitute.For<ILogger<PageModelAuthorisationPolicy>>();
        _policy = new PageModelAuthorisationPolicy(_logger);

        _contentfulService = Substitute.For<IContentfulService>();

        _httpContext = Substitute.For<HttpContext>();

        var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        _httpContext.RequestServices.GetService(typeof(IServiceScopeFactory)).Returns(serviceScopeFactory);
        var serviceProvider = Substitute.For<IServiceProvider>();
        var serviceScope = Substitute.For<IServiceScope>();
        serviceScope.ServiceProvider.Returns(serviceProvider);
        serviceProvider.GetService(typeof(IContentfulService)).Returns(_contentfulService);

        var asyncServiceScope = new AsyncServiceScope(serviceScope);
        serviceScopeFactory.CreateAsyncScope().Returns(asyncServiceScope);

        _httpContext.Request.RouteValues = new RouteValueDictionary
        {
            [PageModelAuthorisationPolicy.RoutesValuesRouteNameKey] = "/slug",
            [PageModelAuthorisationPolicy.RouteValuesControllerNameKey] = "Pages",
        };

        _httpContext.Items = new Dictionary<object, object?>();

        _authContext = new AuthorizationHandlerContext([new PageAuthorisationRequirement()], new ClaimsPrincipal(), _httpContext);
    }

    [Fact]
    public async Task Should_Success_If_Page_Does_Not_Require_Authorisation()
    {
        _contentfulService.GetPageBySlugAsync(Arg.Any<string>()).Returns(callInfo => AuthNotRequiredPage);

        await _policy.HandleAsync(_authContext);

        Assert.True(_authContext.HasSucceeded);
    }

    [Fact]
    public async Task Should_Set_HttpContext_Item_For_Page()
    {
        var testPage = new PageEntry()
        {
            RequiresAuthorisation = false,
            Slug = "TestingSlug"
        };

        _contentfulService.GetPageBySlugAsync(Arg.Any<string>()).Returns(callInfo => testPage);

        await _policy.HandleAsync(_authContext);

        var httpContext = _authContext.Resource as HttpContext;
        Assert.NotNull(httpContext);
        var pageObject = httpContext.Items[nameof(PageEntry)];

        Assert.NotNull(pageObject);

        var page = pageObject as PageEntry;

        Assert.NotNull(page);
        Assert.Equal(testPage, page);
    }

    [Fact]
    public async Task Should_Succeed_If_Page_Requires_Authorisation_And_User_Authenticated()
    {
        _contentfulService.GetPageBySlugAsync(Arg.Any<string>()).Returns(callInfo => AuthRequiredPage);

        var claimsIdentity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "Name")], CookieAuthenticationDefaults.AuthenticationScheme);

        _authContext.User.AddIdentity(claimsIdentity);

        await _policy.HandleAsync(_authContext);

        Assert.False(_authContext.HasSucceeded);
    }

    [Fact]
    public async Task Should_Fail_If_Page_Requires_Authorisation_And_User_Not_Authenticated()
    {
        _contentfulService.GetPageBySlugAsync(Arg.Any<string>()).Returns(callInfo => AuthRequiredPage);

        await _policy.HandleAsync(_authContext);

        Assert.False(_authContext.HasSucceeded);
    }

    [Fact]
    public async Task Should_LogError_When_Resource_Not_HttpContext()
    {
        _authContext = new AuthorizationHandlerContext([new PageAuthorisationRequirement()], new ClaimsPrincipal(), null);
        await _policy.HandleAsync(_authContext);

        var receivedLoggerMessages = _logger.GetMatchingReceivedMessages("Expected resource to be HttpContext but received (null)", LogLevel.Error);
        Assert.Single(receivedLoggerMessages);
    }

    [Fact]
    public async Task Should_Set_Route_Value_When_Null()
    {
        _contentfulService.GetPageBySlugAsync(Arg.Any<string>()).Returns(callInfo => AuthRequiredPage);

        _httpContext.Request.RouteValues.Remove(PageModelAuthorisationPolicy.RoutesValuesRouteNameKey);

        await _policy.HandleAsync(_authContext);

        Assert.True(_httpContext.Request.RouteValues.ContainsKey(PageModelAuthorisationPolicy.RoutesValuesRouteNameKey));
        Assert.Equal("/", _httpContext.Request.RouteValues[PageModelAuthorisationPolicy.RoutesValuesRouteNameKey]);
    }

    [Fact]
    public async Task Should_Fail_When_NotPagesController_And_UserNotAuthenticated()
    {
        _httpContext.Request.RouteValues.Remove(PageModelAuthorisationPolicy.RoutesValuesRouteNameKey);
        _httpContext.Request.RouteValues.Remove(PageModelAuthorisationPolicy.RouteValuesControllerNameKey);

        await _policy.HandleAsync(_authContext);

        Assert.False(_authContext.HasSucceeded);
    }

    [Fact]
    public async Task Should_Succeed_When_Page_Does_Not_Exist()
    {
        _contentfulService.GetPageBySlugAsync(Arg.Any<string>())
            .ThrowsAsync(new ContentfulDataUnavailableException("Arbitrary exception text"));
        await _policy.HandleAsync(_authContext);
        Assert.True(_authContext.HasSucceeded);
    }

    [Fact]
    public async Task Should_Success_When_NotPagesController_And_UserAuthenticated()
    {
        var claimsIdentity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "Name")], CookieAuthenticationDefaults.AuthenticationScheme);
        _authContext.User.AddIdentity(claimsIdentity);

        _httpContext.Request.RouteValues.Remove(PageModelAuthorisationPolicy.RoutesValuesRouteNameKey);
        _httpContext.Request.RouteValues.Remove(PageModelAuthorisationPolicy.RouteValuesControllerNameKey);

        await _policy.HandleAsync(_authContext);

        Assert.False(_authContext.HasSucceeded);
    }
}
