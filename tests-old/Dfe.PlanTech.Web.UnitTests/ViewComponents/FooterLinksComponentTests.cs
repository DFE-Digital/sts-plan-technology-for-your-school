using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.ViewComponents;

public class FooterLinksComponentTests
{
    private readonly NavigationLink[] _navigationLinks = new[]
    {
        new NavigationLink() { DisplayText = "Testing", Href = "/testing" },
    };

    private readonly ILogger<FooterLinksViewComponent> _logger =
        new NullLoggerFactory().CreateLogger<FooterLinksViewComponent>();

    [Fact]
    public async Task It_Should_Retrieve_NavigationLinks()
    {
        var getNavQuery = Substitute.For<IGetNavigationQuery>();

        getNavQuery.GetNavigationLinks(Arg.Any<CancellationToken>()).Returns(_navigationLinks);

        var footerLinks = new FooterLinksViewComponent(getNavQuery, _logger);

        var result = await footerLinks.InvokeAsync();

        var model = (result as ViewViewComponentResult)?.ViewData?.Model;

        Assert.NotNull(model);

        var unboxed = model as NavigationLink[];

        Assert.Equal(_navigationLinks, unboxed);
    }

    [Fact]
    public async Task It_Should_Return_EmptyArray_When_Exception()
    {
        Func<IEnumerable<NavigationLink>> getNavLinksWithError = () =>
            throw new Exception("Error getting Contentful data");

        var getNavQuery = Substitute.For<IGetNavigationQuery>();

        getNavQuery
            .GetNavigationLinks(Arg.Any<CancellationToken>())
            .Returns((callinfo) => getNavLinksWithError());

        var footerLinks = new FooterLinksViewComponent(getNavQuery, _logger);

        var result = await footerLinks.InvokeAsync();

        var model = (result as ViewViewComponentResult)?.ViewData?.Model;

        Assert.NotNull(model);

        var unboxed = model as NavigationLink[];

        Assert.NotNull(unboxed);
        Assert.Empty(unboxed);
    }
}
