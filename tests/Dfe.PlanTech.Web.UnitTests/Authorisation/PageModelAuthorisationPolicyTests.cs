using System.Security.Claims;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.Authorisation;

public class PageModelAuthorisationPolicyTests
{
  private IGetPageQuery _getPageQuery;
  private PageModelAuthorisationPolicy _policy;
  private AuthorizationHandlerContext _authContext;

  public PageModelAuthorisationPolicyTests()
  {
    _policy = new PageModelAuthorisationPolicy(new NullLoggerFactory());

    _getPageQuery = Substitute.For<IGetPageQuery>();

    var httpContext = Substitute.For<HttpContext>();

    var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
    httpContext.RequestServices.GetService(typeof(IServiceScopeFactory)).Returns(serviceScopeFactory);
    var serviceProvider = Substitute.For<IServiceProvider>();
    var serviceScope = Substitute.For<IServiceScope>();
    serviceScope.ServiceProvider.Returns(serviceProvider);
    var asyncServiceScope = new AsyncServiceScope(serviceScope);

    serviceScopeFactory.CreateAsyncScope().Returns(asyncServiceScope);
    serviceProvider.GetService(typeof(IGetPageQuery)).Returns(_getPageQuery);

    httpContext.Request.RouteValues = new RouteValueDictionary();
    httpContext.Request.RouteValues[PageModelAuthorisationPolicy.ROUTE_NAME] = "/";

    httpContext.Items = new Dictionary<object, object?>();

    _authContext = new AuthorizationHandlerContext(new[] { new PageAuthorisationRequirement() }, new ClaimsPrincipal(), httpContext);
  }

  [Fact]
  public async Task Should_Success_If_Page_Does_Not_Require_Authorisation()
  {
    _getPageQuery.GetPageBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(callInfo => new Page()
    {
      RequiresAuthorisation = false
    });

    await _policy.HandleAsync(_authContext);

    Assert.True(_authContext.HasSucceeded);
  }


  [Fact]
  public async Task Should_Set_HttpContext_Item_For_Page()
  {
    var testPage = new Page()
    {
      RequiresAuthorisation = false,
      Slug = "TestingSlug"
    };
    _getPageQuery.GetPageBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(callInfo => testPage);

    await _policy.HandleAsync(_authContext);

    var httpContext = _authContext.Resource as HttpContext;
    Assert.NotNull(httpContext);
    var pageObject = httpContext.Items[nameof(Page)];

    Assert.NotNull(pageObject);

    var page = pageObject as Page;

    Assert.NotNull(page);
    Assert.Equal(testPage, page);
  }

  [Fact]
  public async Task Should_Succeed_If_Page_Requires_Authorisation_And_User_Authenticated()
  {
    _getPageQuery.GetPageBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(callInfo => new Page()
    {
      RequiresAuthorisation = true
    });

    var claimsIdentity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "Name") }, CookieAuthenticationDefaults.AuthenticationScheme);

    _authContext.User.AddIdentity(claimsIdentity);

    await _policy.HandleAsync(_authContext);

    Assert.False(_authContext.HasSucceeded);
  }

  [Fact]
  public async Task Should_Fail_If_Page_Requires_Authorisation_And_User_Not_Authenticated()
  {
    _getPageQuery.GetPageBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(callInfo => new Page()
    {
      RequiresAuthorisation = true
    });

    await _policy.HandleAsync(_authContext);

    Assert.False(_authContext.HasSucceeded);
  }
}
