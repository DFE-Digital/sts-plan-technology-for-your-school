using Dfe.PlanTech.Core.Contentful.Models;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.PagesViewBuilderTests;

public class BuildNotFoundViewModelAsyncTests : PagesViewBuilderTestBase
{
    [Fact]
    public async Task BuildNotFoundViewModelAsync_WhenContactLinkExists_ReturnsViewModelWithContactLinkHref()
    {
        // Arrange
        _contentfulService
            .GetLinkByIdAsync(ContactLinkId)
            .Returns(new NavigationLinkEntry { Href = "/contact-us" });
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildNotFoundViewModelAsync();

        // Assert
        Assert.Equal("/contact-us", result.ContactLinkHref);
    }

    [Fact]
    public async Task BuildNotFoundViewModelAsync_WhenContactLinkDoesNotExist_ReturnsViewModelWithoutContactLinkHref()
    {
        // Arrange
        _contentfulService.GetLinkByIdAsync(ContactLinkId).Returns((NavigationLinkEntry)null!);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildNotFoundViewModelAsync();

        // Assert
        Assert.Null(result.ContactLinkHref);
    }
}
