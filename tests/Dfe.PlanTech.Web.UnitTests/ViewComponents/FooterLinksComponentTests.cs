using Castle.Core.Logging;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.ViewComponents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.ViewComponents;

public class FooterLinksComponentTests
{
  private NavigationLink[] _navigationLinks = new[]{
    new NavigationLink(){
      DisplayText = "Testing",
      Href = "/testing"
    }
  };

  private readonly ILogger<FooterLinks> _logger = new NullLoggerFactory().CreateLogger<FooterLinks>();

  [Fact]
  public async Task It_Should_Retrieve_NavigationLinks()
  {
    var getNavQuery = Substitute.For<IGetNavigationQuery>();

    getNavQuery.GetNavigationLinks(Arg.Any<CancellationToken>())
              .Returns(_navigationLinks);

    var footerLinks = new FooterLinks(getNavQuery, _logger);

    var result = await footerLinks.InvokeAsync();

    var model = (result as ViewViewComponentResult)?.ViewData?.Model;

    Assert.NotNull(model);

    var unboxed = model as NavigationLink[];

    Assert.Equal(_navigationLinks, unboxed);
  }

  [Fact]
  public async Task It_Should_Return_EmptyArray_When_Exception()
  {
    Func<IEnumerable<NavigationLink>> getNavLinksWithError = () => throw new Exception("Error getting Contentful data");
    
    var getNavQuery = Substitute.For<IGetNavigationQuery>();

    getNavQuery.GetNavigationLinks(Arg.Any<CancellationToken>())
              .Returns((callinfo) => getNavLinksWithError());

    var footerLinks = new FooterLinks(getNavQuery, _logger);

    var result = await footerLinks.InvokeAsync();

    var model = (result as ViewViewComponentResult)?.ViewData?.Model;

    Assert.NotNull(model);

    var unboxed = model as NavigationLink[];

    Assert.NotNull(unboxed);
    Assert.Empty(unboxed);
  }

}