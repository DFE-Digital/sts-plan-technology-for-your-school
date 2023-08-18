using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.ViewComponents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using NSubstitute;
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

  [Fact]
  public async Task It_Should_Retrieve_NavigationLinks()
  {
    var getNavQuery = Substitute.For<IGetNavigationQuery>();

    getNavQuery.GetNavigationLinks(Arg.Any<CancellationToken>())
              .Returns(_navigationLinks);

    var footerLinks = new FooterLinks(getNavQuery);

    var result = await footerLinks.InvokeAsync();

    var model = (result as ViewViewComponentResult)?.ViewData?.Model;

    Assert.NotNull(model);

    var unboxed = model as NavigationLink[];

    Assert.Equal(_navigationLinks, unboxed);
  }
}