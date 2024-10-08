using Dfe.ContentSupport.Web.Controllers;
using Dfe.ContentSupport.Web.Models.Mapped;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfe.ContentSupport.Web.Tests.Controllers;

public class ContentControllerTests
{
    private readonly Mock<ILogger<ContentController>> _loggerMock = new();
    private readonly Mock<IContentService> _contentServiceMock = new();

    private ContentController GetController()
    {
        return new ContentController(_contentServiceMock.Object, new LayoutService(), _loggerMock.Object);
    }

    [Fact]
    public async Task Index_NoSlug_Returns_ErrorAction()
    {
        var sut = GetController();

        var result = await sut.Index(string.Empty);

        result.Should().BeOfType<RedirectToActionResult>();
        (result as RedirectToActionResult)!.ActionName.Should().BeEquivalentTo("error");

        _loggerMock.Verify(
               x => x.Log(
                   LogLevel.Error,
                   It.IsAny<EventId>(),
                   It.Is<It.IsAnyType>((o, t) => o.ToString() != null && o.ToString()!.StartsWith($"No slug received for C&S {nameof(ContentController)} {nameof(sut.Index)}")),
                   It.IsAny<Exception>(),
                   It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
               Times.Once);
    }

    [Fact]
    public async Task Index_Calls_Service_GetContent()
    {
        const string dummySlug = "dummySlug";
        const bool isPreview = true;
        var sut = GetController();

        await sut.Index(dummySlug, "", isPreview);

        _contentServiceMock.Verify(o => o.GetContent(dummySlug, isPreview), Times.Once);
    }

    [Fact]
    public async Task Index_NullResponse_ReturnsErrorAction()
    {
        var slug = "slug";
        _contentServiceMock.Setup(o => o.GetContent(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync((CsPage?)null);

        var sut = GetController();

        var result = await sut.Index(slug);

        result.Should().BeOfType<RedirectToActionResult>();
        (result as RedirectToActionResult)!.ActionName.Should().BeEquivalentTo("error");

        _loggerMock.Verify(x => x.Log(
           LogLevel.Error,
           It.IsAny<EventId>(),
           It.Is<It.IsAnyType>((o, t) => o.ToString() != null && o.ToString()!.Equals($"Failed to load content for C&S page {slug}; no content received.")),
           It.IsAny<Exception>(),
           It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
       Times.Once);
    }

    [Fact]
    public async Task Index_ExceptionThrown_ReturnsErrorAction()
    {
        var slug = "slug";
        _contentServiceMock.Setup(o => o.GetContent(It.IsAny<string>(), It.IsAny<bool>()))
            .ThrowsAsync(new Exception("An exception occurred loading content"));

        var sut = GetController();

        var result = await sut.Index(slug);

        result.Should().BeOfType<RedirectToActionResult>();
        (result as RedirectToActionResult)!.ActionName.Should().BeEquivalentTo("error");

        _loggerMock.Verify(x => x.Log(
           LogLevel.Error,
           It.IsAny<EventId>(),
           It.Is<It.IsAnyType>((o, t) => o.ToString() != null && o.ToString()!.Equals($"Error loading C&S content page {slug}")),
           It.IsAny<Exception>(),
           It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
       Times.Once);
    }

    [Fact]
    public async Task Index_WithSlug_Returns_View()
    {
        _contentServiceMock.Setup(o => o.GetContent(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new CsPage());

        var sut = GetController();
        var result = await sut.Index("slug1");

        result.Should().BeOfType<ViewResult>();
        (result as ViewResult)!.Model.Should().BeOfType<CsPage>();
    }

    [Fact]
    public void Error_Returns_ErrorView()
    {
        var sut = GetController();
        sut.ControllerContext.HttpContext = new DefaultHttpContext();
        var result = sut.Error();

        result.Should().BeOfType<ViewResult>();
        (result as ViewResult)!.Model.Should().BeOfType<ErrorViewModel>();
    }
}