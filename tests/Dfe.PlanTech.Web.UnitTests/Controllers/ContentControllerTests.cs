using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped;
using Dfe.PlanTech.Web.Content;
using Dfe.PlanTech.Web.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;
using NSubstitute;
using NSubstitute.ReceivedExtensions;

namespace Dfe.ContentSupport.Web.Tests.Controllers;

public class ContentControllerTests
{
    private readonly ILogger<ContentController> _loggerMock = Substitute.For<ILogger<ContentController>>();
    private readonly IContentService _contentServiceMock = Substitute.For<IContentService>();

    private ContentController GetController()
    {
        return new ContentController(_contentServiceMock, new LayoutService(), _loggerMock);
    }

    [Fact]
    public async Task Index_NoSlug_Returns_ErrorAction()
    {
        var sut = GetController();

        var result = await sut.Index(string.Empty, "", null, default); // Provide all parameters explicitly

        result.Should().BeOfType<RedirectToActionResult>();
        (result as RedirectToActionResult)!.ActionName.Should().BeEquivalentTo(PagesController.NotFoundPage);

        _loggerMock.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<Object>(o => o.ToString() != null && o.ToString()!.StartsWith($"No slug received for C&S {nameof(ContentController)} {nameof(sut.Index)}")),
            Arg.Any<Exception>(),
            Arg.Any<Func<Object, Exception?, string>>()
        );
    }

    [Fact]
    public async Task Index_Calls_Service_GetContent()
    {
        const string dummySlug = "dummySlug";
        var sut = GetController();

        await sut.Index(dummySlug, "", null, default);

        await _contentServiceMock.Received(1).GetContent(dummySlug, default);
    }

    [Fact]
    public async Task Index_NullResponse_ReturnsErrorAction()
    {
        var slug = "slug";

        _contentServiceMock.GetContent(Arg.Any<String>(), Arg.Any<CancellationToken>()).Returns((CsPage?)null);

        var sut = GetController();

        var result = await sut.Index(slug, "", null, default);

        result.Should().BeOfType<RedirectToActionResult>();
        (result as RedirectToActionResult)!.ActionName.Should().BeEquivalentTo(PagesController.NotFoundPage);


        _loggerMock.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<Object>(o => o.ToString() != null && o.ToString()!.Equals($"Failed to load content for C&S page {slug}; no content received.")),
            Arg.Any<Exception>(),
            Arg.Any<Func<Object, Exception?, string>>()
        );
    }

    [Fact]
    public async Task Index_ExceptionThrown_ReturnsErrorAction()
    {
        var slug = "slug";

        _contentServiceMock.GetContent(Arg.Any<String>(), Arg.Any<CancellationToken>()).Returns(Task.FromException(new Exception("An exception occurred loading content")));

        var sut = GetController();

        var result = await sut.Index(slug, "", null, default);

        result.Should().BeOfType<RedirectToActionResult>();
        (result as RedirectToActionResult)!.ActionName.Should().BeEquivalentTo("error");

        _loggerMock.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<Object>(o => o.ToString() != null && o.ToString()!.Equals($"Error loading C&S content page {slug}")),
            Arg.Any<Exception>(),
            Arg.Any<Func<Object, Exception?, string>>()
        );
    }

    [Fact]
    public async Task Index_WithSlug_Returns_View()
    {
        _contentServiceMock.GetContent(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new CsPage());

        var sut = GetController();
        var result = await sut.Index("slug1", "", null, default);

        result.Should().BeOfType<ViewResult>();
        (result as ViewResult)!.Model.Should().BeOfType<CsPage>();
    }
}
