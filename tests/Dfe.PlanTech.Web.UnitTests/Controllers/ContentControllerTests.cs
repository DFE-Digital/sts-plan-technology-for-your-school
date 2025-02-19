using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped;
using Dfe.PlanTech.Web.Content;
using Dfe.PlanTech.Web.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReceivedExtensions;
using Xunit;

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
        var controller = GetController();

        var index = async () => await controller.Index(string.Empty, "", null, default);

        await Assert.ThrowsAsync<KeyNotFoundException>(index);

        _loggerMock.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<Object>(o => o.ToString() != null && o.ToString()!.StartsWith($"No slug received for C&S {nameof(ContentController)} {nameof(controller.Index)}")),
            Arg.Any<Exception>(),
            Arg.Any<Func<Object, Exception?, string>>()
        );
    }

    [Fact]
    public async Task Index_Calls_Service_GetContent()
    {
        const string dummySlug = "dummySlug";
        var controller = GetController();

        await controller.Index(dummySlug, "", null, default);

        await _contentServiceMock.Received(1).GetContent(dummySlug, default);
    }

    [Fact]
    public async Task Index_NullResponse_ReturnsErrorAction()
    {
        var slug = "slug";

        _contentServiceMock.GetContent(Arg.Any<String>(), Arg.Any<CancellationToken>()).Returns((CsPage?)null);

        var controller = GetController();

        var result = await controller.Index(slug, "", null, default);

        result.Should().BeOfType<RedirectToActionResult>();
        (result as RedirectToActionResult)!.ActionName.Should().BeEquivalentTo(ContentController.ErrorActionName);

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

        _contentServiceMock.GetContent(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException<CsPage?>(new Exception("An exception occurred loading content")));

        var controller = GetController();

        var result = await controller.Index(slug, "", null, default);

        result.Should().BeOfType<RedirectToActionResult>();
        (result as RedirectToActionResult)!.ActionName.Should().BeEquivalentTo("error");

        _loggerMock.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString() != null && o.ToString()!.Equals($"Error loading C&S content page {slug}")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>()
        );
    }

    [Fact]
    public async Task Index_WithSlug_Returns_View()
    {
        _contentServiceMock.GetContent(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new CsPage());

        var controller = GetController();
        var result = await controller.Index("slug1", "", null, default);

        result.Should().BeOfType<ViewResult>();
        (result as ViewResult)!.Model.Should().BeOfType<CsPage>();
    }
}
