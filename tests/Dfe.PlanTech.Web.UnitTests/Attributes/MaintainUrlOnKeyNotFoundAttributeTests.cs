using Dfe.PlanTech.Application.Configuration;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Web.Attributes;
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

    private static MaintainUrlOnKeyNotFoundAttribute BuildSut(
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
    public void OnExceptionAsync_When_ContentfulDataUnavailable_Returns_NotFound_View_And_Sets_ContactLink()
    {
        // Arrange
        var linkId = "contact-link-id";
        var contentful = Substitute.For<IContentfulService>();
        contentful.GetLinkByIdAsync(linkId).Returns(new NavigationLinkEntry { Href = "/contact-us" });

        var sut = BuildSut(linkId, contentful);
        var ctx = BuildExceptionContext();
        ctx.Exception = new ContentfulDataUnavailableException("boom");
    }
}
