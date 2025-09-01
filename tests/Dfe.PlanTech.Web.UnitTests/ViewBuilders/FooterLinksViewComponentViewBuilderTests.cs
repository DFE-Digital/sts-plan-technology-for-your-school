using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.ViewBuilders;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders;

public class FooterLinksViewComponentViewBuilderTests
{
    private static FooterLinksViewComponentViewBuilder CreateServiceUnderTest(
        IContentfulService? contentful = null,
        ICurrentUser? currentUser = null,
        ILogger<FooterLinksViewComponentViewBuilder>? mockLogger = null)
    {
        contentful ??= Substitute.For<IContentfulService>();
        currentUser ??= Substitute.For<ICurrentUser>();
        mockLogger ??= Substitute.For<ILogger<FooterLinksViewComponentViewBuilder>>();

        return new FooterLinksViewComponentViewBuilder(
            mockLogger,
            contentful,
            currentUser
        );
    }

    [Fact]
    public async Task GetNavigationLinksAsync_Returns_Links_From_Contentful()
    {
        // Arrange
        var contentful = Substitute.For<IContentfulService>();
        var expected = new List<NavigationLinkEntry>
        {
            new NavigationLinkEntry { Sys = new SystemDetails("n1"), InternalName = "Home" },
            new NavigationLinkEntry { Sys = new SystemDetails("n2"), InternalName = "About" },
        };
        contentful.GetNavigationLinksAsync().Returns(expected);

        var sut = CreateServiceUnderTest(contentful);

        // Act
        var result = await sut.GetNavigationLinksAsync();

        // Assert
        Assert.Same(expected, result); // should pass-through the same list instance
        await contentful.Received(1).GetNavigationLinksAsync();
    }

    [Fact]
    public async Task GetNavigationLinksAsync_On_Exception_Logs_Critical_And_Returns_Empty_List()
    {
        // Arrange
        var contentful = Substitute.For<IContentfulService>();
        var logger = Substitute.For<ILogger<FooterLinksViewComponentViewBuilder>>();
        contentful.GetNavigationLinksAsync().ThrowsAsync(new InvalidOperationException("boom"));

        var sut = CreateServiceUnderTest(contentful, mockLogger: logger);

        // Act
        await sut.GetNavigationLinksAsync();

        // Assert: one Critical log with the expected message fragment and exception
        logger.Received(1).Log(
            LogLevel.Critical,
            Arg.Any<EventId>(),
            Arg.Is<object>(o =>
                o.ToString()!.Contains("Error retrieving navigation links for footer")),
            Arg.Is<Exception?>(e => e!.Message.Equals("boom")),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
