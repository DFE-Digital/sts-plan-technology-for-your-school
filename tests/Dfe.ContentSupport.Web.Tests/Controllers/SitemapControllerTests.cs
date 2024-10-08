using Dfe.ContentSupport.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.ContentSupport.Web.Tests.Controllers;

public class SitemapControllerTests
{
    private readonly Mock<IContentService> _contentServiceMock = new();

    private SitemapController GetController()
    {
        return new SitemapController(_contentServiceMock.Object);
    }

    [Fact]
    public async Task Sitemap_Calls_Service_GenerateSitemap()
    {
        var sut = GetController();
        sut.ControllerContext.HttpContext = new DefaultHttpContext();
        var baseUrl =
            $"{sut.ControllerContext.HttpContext.Request.Scheme}://{sut.ControllerContext.HttpContext.Request.Host}/";

        await sut.Sitemap();
        _contentServiceMock.Verify(o => o.GenerateSitemap(baseUrl), Times.Once);
    }

    [Fact]
    public async Task Sitemap_Calls_Returns_ContentResult_XmlModel()
    {
        const string sitemap = "dummy";
        _contentServiceMock.Setup(o => o.GenerateSitemap(It.IsAny<string>()))
            .ReturnsAsync(sitemap);

        var expected = new ContentResult
        {
            Content = sitemap,
            ContentType = "application/xml"
        };

        var sut = GetController();
        sut.ControllerContext.HttpContext = new DefaultHttpContext();

        var result = await sut.Sitemap();

        result.Should().BeEquivalentTo(expected);
    }
}