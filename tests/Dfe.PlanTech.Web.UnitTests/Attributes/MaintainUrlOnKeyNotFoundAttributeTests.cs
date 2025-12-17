using Dfe.PlanTech.Application.Configuration;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Web.Attributes;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Attributes;

public class MaintainUrlOnKeyNotFoundAttributeTests
{
    private static ExceptionContext BuildExceptionContext()
    {
        var http = new DefaultHttpContext();
        var actionContext = new ActionContext(
            http,
            new RouteData(),
            new ActionDescriptor(),
            new ModelStateDictionary());

        return new ExceptionContext(actionContext, new List<IFilterMetadata>());
    }

    private static MaintainUrlOnKeyNotFoundAttribute BuildServiceUnderTest(
        string linkId,
        IContentfulService? contentful = null)
    {
        var opts = Options.Create(new ContactOptionsConfiguration { LinkId = linkId });
        var svc = contentful ?? Substitute.For<IContentfulService>();
        return new MaintainUrlOnKeyNotFoundAttribute(opts, svc);
    }

    // ---------- Ctor guards ----------

    [Fact]
    public void Ctor_Throws_When_Options_Null()
    {
        var svc = Substitute.For<IContentfulService>();
        Assert.Throws<ArgumentNullException>(() => new MaintainUrlOnKeyNotFoundAttribute(null!, svc));
    }

    [Fact]
    public void Ctor_Throws_When_ContentfulService_Null()
    {
        var opts = Options.Create(new ContactOptionsConfiguration { LinkId = "link-1" });
        Assert.Throws<ArgumentNullException>(() => new MaintainUrlOnKeyNotFoundAttribute(opts, null!));
    }

    // ---------- Handles ContentfulDataUnavailableException ----------

    [Fact]
    public async Task OnExceptionAsync_When_ContentfulDataUnavailable_Returns_NotFound_View_And_Sets_ContactLink()
    {
        // Arrange
        var linkId = "contact-link-id";
        var contentful = Substitute.For<IContentfulService>();
        contentful.GetLinkByIdAsync(linkId).Returns(new NavigationLinkEntry { Sys = new SystemDetails(linkId), Href = "/contact-us" });

        var sut = BuildServiceUnderTest(linkId, contentful);
        var ctx = BuildExceptionContext();
        ctx.Exception = new ContentfulDataUnavailableException("boom");

        await sut.OnExceptionAsync(ctx);

        Assert.True(ctx.ExceptionHandled);
        var view = Assert.IsType<ViewResult>(ctx.Result);
        Assert.Equal("NotFoundError", view.ViewName);
        var vm = Assert.IsType<NotFoundViewModel>(view.ViewData.Model);
        Assert.Equal("/contact-us", vm.ContactLinkHref);
        await contentful.Received(1).GetLinkByIdAsync(linkId);
    }

    // ---------- Handles KeyNotFoundException (NOT organisation) ----------

    [Fact]
    public async Task OnExceptionAsync_When_KeyNotFound_And_NotOrganisation_Handled_With_View_And_ContactLink()
    {
        // Arrange
        var linkId = "contact-link-id";
        var contentful = Substitute.For<IContentfulService>();
        contentful.GetLinkByIdAsync(linkId)
            .Returns(new NavigationLinkEntry { Sys = new SystemDetails(linkId), Href = "/help" });

        var sut = BuildServiceUnderTest(linkId, contentful);
        var ctx = BuildExceptionContext();
        ctx.Exception = new KeyNotFoundException("missing some key");

        // Act
        await sut.OnExceptionAsync(ctx);

        // Assert
        Assert.True(ctx.ExceptionHandled);
        var view = Assert.IsType<ViewResult>(ctx.Result);
        Assert.Equal("NotFoundError", view.ViewName);
        var vm = Assert.IsType<NotFoundViewModel>(view.ViewData.Model);
        Assert.Equal("/help", vm.ContactLinkHref);
        await contentful.Received(1).GetLinkByIdAsync(linkId);
    }

    // ---------- Does NOT handle KeyNotFoundException mentioning Organisation ----------

    [Fact]
    public async Task OnExceptionAsync_When_KeyNotFound_For_Organisation_Is_NotHandled()
    {
        // Arrange
        var linkId = "contact-link-id";
        var contentful = Substitute.For<IContentfulService>();
        var sut = BuildServiceUnderTest(linkId, contentful);
        var ctx = BuildExceptionContext();
        ctx.Exception = new KeyNotFoundException($"Could not find {ClaimConstants.Organisation}");

        // Act
        await sut.OnExceptionAsync(ctx);

        // Assert
        Assert.False(ctx.ExceptionHandled);
        Assert.Null(ctx.Result);
        await contentful.DidNotReceiveWithAnyArgs().GetLinkByIdAsync(default!);
    }

    // ---------- Does NOT handle unrelated exceptions ----------

    [Fact]
    public async Task OnExceptionAsync_When_Other_Exception_Is_NotHandled()
    {
        // Arrange
        var linkId = "contact-link-id";
        var contentful = Substitute.For<IContentfulService>();
        var sut = BuildServiceUnderTest(linkId, contentful);
        var ctx = BuildExceptionContext();
        ctx.Exception = new InvalidOperationException("nope");

        // Act
        await sut.OnExceptionAsync(ctx);

        // Assert
        Assert.False(ctx.ExceptionHandled);
        Assert.Null(ctx.Result);
        await contentful.DidNotReceiveWithAnyArgs().GetLinkByIdAsync(default!);
    }

    // ---------- Safe when Contentful returns null link ----------

    [Fact]
    public async Task OnExceptionAsync_When_Contentful_Returns_Null_Link_Sets_Null_Href_And_Handles()
    {
        // Arrange
        var linkId = "contact-link-id";
        var contentful = Substitute.For<IContentfulService>();
        contentful.GetLinkByIdAsync(linkId).Returns(new NavigationLinkEntry());

        var sut = BuildServiceUnderTest(linkId, contentful);
        var ctx = BuildExceptionContext();
        ctx.Exception = new ContentfulDataUnavailableException("boom");

        // Act
        await sut.OnExceptionAsync(ctx);

        // Assert
        Assert.True(ctx.ExceptionHandled);
        var view = Assert.IsType<ViewResult>(ctx.Result);
        Assert.Equal("NotFoundError", view.ViewName);
        var vm = Assert.IsType<NotFoundViewModel>(view.ViewData.Model);
        Assert.Null(vm.ContactLinkHref);
        await contentful.Received(1).GetLinkByIdAsync(linkId);
    }
}
