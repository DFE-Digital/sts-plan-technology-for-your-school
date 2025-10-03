using Dfe.PlanTech.Application.Configuration;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.ViewBuilders;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders;

public class PagesViewBuilderTests
{
    // ---------- helpers ----------
    private sealed class TestController : Controller
    {
        public TestController()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/test-path";

            ControllerContext = new ControllerContext();
            ControllerContext.HttpContext = httpContext;
            TempData = new TempDataDictionary(httpContext, Substitute.For<ITempDataProvider>());
        }
    }

    private static IOptions<ContactOptionsConfiguration> ContactOpts(string linkId = "contact-1")
        => Options.Create(new ContactOptionsConfiguration { LinkId = linkId });

    private static IOptions<ErrorPagesConfiguration> ErrorOpts(string internalId = "err-500")
        => Options.Create(new ErrorPagesConfiguration { InternalErrorPageId = internalId });

    private static PagesViewBuilder CreateServiceUnderTest(
        IOptions<ContactOptionsConfiguration>? contact = null,
        IOptions<ErrorPagesConfiguration>? errors = null,
        IContentfulService? contentful = null,
        ICurrentUser? currentUser = null,
        ILogger<BaseViewBuilder>? logger = null)
    {
        contact ??= ContactOpts();
        errors ??= ErrorOpts();
        contentful ??= Substitute.For<IContentfulService>();
        currentUser ??= Substitute.For<ICurrentUser>();
        logger ??= NullLogger<BaseViewBuilder>.Instance;

        // sensible defaults
        currentUser.IsMat.Returns(false);
        currentUser.IsAuthenticated.Returns(true);
        currentUser.Organisation.Returns(new OrganisationModel { Name = "Acme Academy" });

        return new PagesViewBuilder(logger, contact, errors, contentful, currentUser);
    }

    private static PageEntry MakePage(string slug, bool isLanding = false, string? title = "My Page", bool displayOrg = false, string? id = "pg-1")
        => new PageEntry
        {
            Sys = new SystemDetails(id!),
            Slug = slug,
            IsLandingPage = isLanding,
            Title = title is null ? null : new ComponentTitleEntry(title),
            DisplayOrganisationName = displayOrg
        };

    private static QuestionnaireCategoryEntry MakeCategory(string header = "Cat")
        => new QuestionnaireCategoryEntry { Header = new ComponentHeaderEntry { Text = header }, Sections = new List<QuestionnaireSectionEntry>() };

    // ---------- ctor guards ----------
    [Fact]
    public void Ctor_Null_ContactOptions_Throws()
    {
        var errors = ErrorOpts();
        var contentful = Substitute.For<IContentfulService>();
        var current = Substitute.For<ICurrentUser>();
        Assert.Throws<ArgumentNullException>(() => new PagesViewBuilder(NullLogger<BaseViewBuilder>.Instance, null!, errors, contentful, current));
    }

    [Fact]
    public void Ctor_Null_ErrorOptions_Throws()
    {
        var contact = ContactOpts();
        var contentful = Substitute.For<IContentfulService>();
        var current = Substitute.For<ICurrentUser>();
        Assert.Throws<ArgumentNullException>(() => new PagesViewBuilder(NullLogger<BaseViewBuilder>.Instance, contact, null!, contentful, current));
    }

    // ---------- RouteBasedOnOrganisationTypeAsync ----------
    [Fact]
    public async Task RouteBasedOnOrganisationType_Redirects_To_SelectSchool_For_Home_And_MAT()
    {
        // Home slug logic uses UrlConstants.HomePage.Replace("/", "")
        var homeSlug = UrlConstants.HomePage.Replace("/", ""); // usually ""
        var page = MakePage(homeSlug, isLanding: false);

        var contentful = Substitute.For<IContentfulService>();
        var current = Substitute.For<ICurrentUser>();

        var sut = CreateServiceUnderTest(contentful: contentful, currentUser: current);
        current.IsMat.Returns(true);

        var controller = new TestController();

        var action = await sut.RouteBasedOnOrganisationTypeAsync(controller, page);

        var redirect = Assert.IsType<RedirectResult>(action);
        Assert.Equal(UrlConstants.SelectASchoolPage, redirect.Url);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationType_LandingPage_Path_Returns_CategoryLanding_View()
    {
        var page = MakePage("networks", isLanding: true);
        var category = MakeCategory("Networking");

        var contentful = Substitute.For<IContentfulService>();
        contentful.GetCategoryBySlugAsync("networks", 4).Returns(category);

        var sut = CreateServiceUnderTest(contentful: contentful);
        var controller = new TestController();
        controller.TempData["SectionName"] = "Some Section";

        var action = await sut.RouteBasedOnOrganisationTypeAsync(controller, page);

        var view = Assert.IsType<ViewResult>(action);
        Assert.Equal(PagesViewBuilder.CategoryLandingPageView, view.ViewName);
        var vm = Assert.IsType<CategoryLandingPageViewModel>(view.Model);
        Assert.Equal("networks", vm.Slug);
        Assert.Equal("Networking", vm.Title.Text);
        Assert.Equal("Some Section", vm.SectionName);

        await contentful.Received(1).GetCategoryBySlugAsync("networks", 4);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationType_LandingPage_Throws_When_Category_Not_Found()
    {
        var page = MakePage("missing", isLanding: true);
        var contentful = Substitute.For<IContentfulService>();

        var sut = CreateServiceUnderTest(contentful: contentful);
        contentful.GetCategoryBySlugAsync("missing", 4).Returns((QuestionnaireCategoryEntry?)null);

        var controller = new TestController();

        await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            sut.RouteBasedOnOrganisationTypeAsync(controller, page));
    }

    [Fact]
    public async Task RouteBasedOnOrganisationType_NormalPage_Sets_Title_And_OrgName_When_Requested_And_Authenticated()
    {
        var page = MakePage("about", isLanding: false, title: "About Us", displayOrg: true);

        var sut = CreateServiceUnderTest();
        var controller = new TestController();

        var action = await sut.RouteBasedOnOrganisationTypeAsync(controller, page);

        var view = Assert.IsType<ViewResult>(action);
        Assert.Equal("Page", view.ViewName);

        var vm = Assert.IsType<PageViewModel>(view.Model);
        Assert.Equal("Acme Academy", vm.OrganisationName);
        Assert.Equal("About Us", controller.ViewData["Title"]); // processed title
        Assert.True(vm.DisplayBlueBanner); // default
    }

    [Fact]
    public async Task RouteBasedOnOrganisationType_NormalPage_NoTitle_Uses_Default_Page_Title()
    {
        var page = MakePage("plain", isLanding: false, title: null, displayOrg: false);

        var sut = CreateServiceUnderTest();
        var controller = new TestController();

        var action = await sut.RouteBasedOnOrganisationTypeAsync(controller, page);

        var view = Assert.IsType<ViewResult>(action);
        Assert.Equal(PageTitleConstants.PlanTechnologyForYourSchool, controller.ViewData["Title"]);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationType_InternalErrorPage_Disables_Blue_Banner()
    {
        var errors = ErrorOpts(internalId: "err-500");
        var page = MakePage("error", isLanding: false, title: "Err", displayOrg: false, id: "err-500");

        var sut = CreateServiceUnderTest(errors: errors);
        var controller = new TestController();

        var action = await sut.RouteBasedOnOrganisationTypeAsync(controller, page);

        var view = Assert.IsType<ViewResult>(action);
        var vm = Assert.IsType<PageViewModel>(view.Model);
        Assert.False(vm.DisplayBlueBanner);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationType_DisplayOrgName_But_NotAuthenticated_Does_Not_Set_Name()
    {
        var page = MakePage("about", isLanding: false, title: "About", displayOrg: true);

        var current = Substitute.For<ICurrentUser>();
        var sut = CreateServiceUnderTest(currentUser: current);
        current.IsAuthenticated.Returns(false);
        current.IsMat.Returns(false);

        var controller = new TestController();

        var action = await sut.RouteBasedOnOrganisationTypeAsync(controller, page);

        var view = Assert.IsType<ViewResult>(action);
        var vm = Assert.IsType<PageViewModel>(view.Model);
        Assert.Null(vm.OrganisationName);
    }

    // ---------- BuildNotFoundViewModel ----------
    [Fact]
    public async Task BuildNotFoundViewModel_Uses_Contact_Link_From_Contentful()
    {
        var contentful = Substitute.For<IContentfulService>();
        contentful.GetLinkByIdAsync("contact-1")
                  .Returns(new NavigationLinkEntry { Href = "/contact" });

        var sut = CreateServiceUnderTest(contentful: contentful);

        var vm = await sut.BuildNotFoundViewModel();

        Assert.Equal("/contact", vm.ContactLinkHref);
        await contentful.Received(1).GetLinkByIdAsync("contact-1");
    }
}
