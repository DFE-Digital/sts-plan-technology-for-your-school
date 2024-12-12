using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped;
using Dfe.PlanTech.Web.Content;
using Dfe.PlanTech.Web.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

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

        var result = await sut.Index(string.Empty, "", null, default); // Provide all parameters explicitly

        result.Should().BeOfType<RedirectToActionResult>();
        (result as RedirectToActionResult)!.ActionName.Should().BeEquivalentTo(PagesController.NotFoundPage);

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
        var sut = GetController();

        await sut.Index(dummySlug, "", null, default);

        _contentServiceMock.Verify(o => o.GetContent(dummySlug, default), Times.Once);
    }

    [Fact]
    public async Task Index_NullResponse_ReturnsErrorAction()
    {
        var slug = "slug";
        _contentServiceMock.Setup(o => o.GetContent(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CsPage?)null);

        var sut = GetController();

        var result = await sut.Index(slug, "", null, default);

        result.Should().BeOfType<RedirectToActionResult>();
        (result as RedirectToActionResult)!.ActionName.Should().BeEquivalentTo(PagesController.NotFoundPage);

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
        _contentServiceMock.Setup(o => o.GetContent(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("An exception occurred loading content"));

        var sut = GetController();

        var result = await sut.Index(slug, "", null, default);

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
        _contentServiceMock.Setup(o => o.GetContent(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CsPage());

        var sut = GetController();
        var result = await sut.Index("slug1", "", null, default);

        result.Should().BeOfType<ViewResult>();
        (result as ViewResult)!.Model.Should().BeOfType<CsPage>();
    }
}