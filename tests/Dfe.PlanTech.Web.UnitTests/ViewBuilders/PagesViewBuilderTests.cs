using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Configuration;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.ViewBuilders;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders;

public class PagesViewBuilderTests
{
    private const string DefaultRecipient = "test@test.com";

    // ---- Substitutes (collaborators)

    private readonly ILogger<BaseViewBuilder> _logger = NullLogger<BaseViewBuilder>.Instance;
    private readonly ICategoryLandingViewComponentViewBuilder _viewBuilder =
        Substitute.For<ICategoryLandingViewComponentViewBuilder>();
    private readonly IContentfulService _contentfulService = Substitute.For<IContentfulService>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IEstablishmentService _establishmentService =
        Substitute.For<IEstablishmentService>();
    private readonly INotifyService _notifyService = Substitute.For<INotifyService>();
    private readonly IRecommendationService _recommendationService =
        Substitute.For<IRecommendationService>();
    private readonly ISubmissionService _submissionService = Substitute.For<ISubmissionService>();

    // ---- Options

    private static IOptions<ContactOptionsConfiguration> ContactOpts(string linkId = "contact-1") =>
        Options.Create(new ContactOptionsConfiguration { LinkId = linkId });

    private static IOptions<ErrorPagesConfiguration> ErrorOpts(string internalId = "err-500") =>
        Options.Create(new ErrorPagesConfiguration { InternalErrorPageId = internalId });

    private static TestController CreateController() => new TestController();

    private PagesViewBuilder CreateServiceUnderTest(
        ILogger<BaseViewBuilder>? logger = null,
        IOptions<ContactOptionsConfiguration>? contact = null,
        IOptions<ErrorPagesConfiguration>? errors = null,
        ICategoryLandingViewComponentViewBuilder? viewBuilder = null,
        IContentfulService? contentful = null,
        ICurrentUser? currentUser = null,
        IEstablishmentService? establishmentService = null,
        INotifyService? notifyService = null,
        ISubmissionService? submissionService = null,
        IRecommendationService? recommendationService = null,
        bool useCurrentUserDefaults = true
    )
    {
        logger ??= NullLogger<BaseViewBuilder>.Instance;
        contact ??= ContactOpts();
        errors ??= ErrorOpts();
        viewBuilder ??= _viewBuilder;
        contentful ??= _contentfulService;
        currentUser ??= _currentUser;
        establishmentService ??= _establishmentService;
        notifyService ??= _notifyService;
        submissionService ??= _submissionService;
        recommendationService ??= _recommendationService;

        if (useCurrentUserDefaults)
        {
            // Sensible defaults
            currentUser.IsMat.Returns(false);
            currentUser.IsAuthenticated.Returns(true);
            currentUser.GetActiveEstablishmentNameAsync().Returns("Acme Academy");
            currentUser.GetActiveEstablishmentUrnAsync().Returns("123456");
        }

        return new PagesViewBuilder(
            logger,
            contact,
            errors,
            contentful,
            currentUser,
            establishmentService,
            notifyService,
            submissionService,
            recommendationService
        );
    }

    private static PageEntry CreatePage(
        string slug,
        bool isLanding = false,
        string? title = "My Page",
        bool displayOrg = false,
        string? id = "pg-1"
    ) =>
        new PageEntry
        {
            Sys = new SystemDetails(id!),
            Slug = slug,
            IsLandingPage = isLanding,
            Title = title is null ? null : new ComponentTitleEntry(title),
            DisplayOrganisationName = displayOrg,
        };

    private static QuestionnaireCategoryEntry CreateCategory(string header = "Cat") =>
        new QuestionnaireCategoryEntry
        {
            Header = new ComponentHeaderEntry { Text = header },
            LandingPage = new PageEntry { Slug = header.ToLower() },
            Sections = new List<QuestionnaireSectionEntry>(),
        };

    private static QuestionnaireCategoryEntry CreateCategory(
        string headerText,
        string? landingPageSlug = null
    )
    {
        var category = new QuestionnaireCategoryEntry
        {
            Header = new ComponentHeaderEntry { Text = headerText },
        };

        if (!string.IsNullOrWhiteSpace(landingPageSlug))
        {
            category.LandingPage = new PageEntry { Slug = landingPageSlug };
        }

        return category;
    }

    // ---------- ctor guards ----------
    [Fact]
    public void Ctor_Null_ContactOptions_Throws()
    {
        var errors = ErrorOpts();
        var contentful = Substitute.For<IContentfulService>();
        var currentUser = Substitute.For<ICurrentUser>();
        var establishmentService = Substitute.For<IEstablishmentService>();
        var notifyService = Substitute.For<INotifyService>();
        var submissionService = Substitute.For<ISubmissionService>();
        var reocmmendationService = Substitute.For<IRecommendationService>();

        Assert.Throws<ArgumentNullException>(() =>
            new PagesViewBuilder(
                NullLogger<BaseViewBuilder>.Instance,
                null!,
                errors,
                contentful,
                currentUser,
                establishmentService,
                notifyService,
                submissionService,
                reocmmendationService
            )
        );
    }

    [Fact]
    public void Ctor_Null_ErrorOptions_Throws()
    {
        var contact = ContactOpts();
        var contentful = Substitute.For<IContentfulService>();
        var currentUser = Substitute.For<ICurrentUser>();
        var establishmentService = Substitute.For<IEstablishmentService>();
        var notifyService = Substitute.For<INotifyService>();
        var submissionService = Substitute.For<ISubmissionService>();
        var reocmmendationService = Substitute.For<IRecommendationService>();

        Assert.Throws<ArgumentNullException>(() =>
            new PagesViewBuilder(
                NullLogger<BaseViewBuilder>.Instance,
                contact,
                null!,
                contentful,
                currentUser,
                establishmentService,
                notifyService,
                submissionService,
                reocmmendationService
            )
        );
    }

    // ---------- RouteBasedOnOrganisationTypeAsync ----------

    [Fact]
    public async Task RouteBasedOnOrganisationType_When_NoSchoolSelected_Then_RedirectsToSelectSchoolPage()
    {
        // Arrange - A MAT user without a selected school, attempting to access the home page.
        // Home slug logic uses UrlConstants.HomePage.Replace("/", "")
        var homeSlug = UrlConstants.HomePage.Replace("/", ""); // usually ""
        var page = CreatePage(homeSlug, isLanding: false);

        var contentful = Substitute.For<IContentfulService>();
        var currentUser = Substitute.For<ICurrentUser>();

        var sut = CreateServiceUnderTest(contentful: contentful, currentUser: currentUser);

        // `CreateServiceUnderTest` sets defaults, so must override here:
        currentUser.IsAuthenticated.Returns(true);
        currentUser.UserOrganisationIsGroup.Returns(true);
        currentUser.UserOrganisationId.Returns(654321); // the ID for the group (MAT)
        currentUser.GroupSelectedSchoolUrn.Returns((string?)null);

        var controller = new TestController();

        // Act
        var action = await sut.RouteBasedOnOrganisationTypeAsync(controller, page);

        // Assert
        var redirect = Assert.IsType<RedirectResult>(action);
        Assert.Equal(UrlConstants.SelectASchoolPage, redirect.Url);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationType_When_SchoolNotInGroup_Then_RedirectsToSelectSchoolPage()
    {
        // Arrange - A MAT user with a selected school outside their group, attempting to access the home page.
        // Home slug logic uses UrlConstants.HomePage.Replace("/", "")
        var homeSlug = UrlConstants.HomePage.Replace("/", ""); // usually ""
        var page = CreatePage(homeSlug, isLanding: false);

        var contentful = Substitute.For<IContentfulService>();
        var currentUser = Substitute.For<ICurrentUser>();
        var establishmentService = Substitute.For<IEstablishmentService>();

        var sut = CreateServiceUnderTest(
            contentful: contentful,
            currentUser: currentUser,
            establishmentService: establishmentService
        );

        // `CreateServiceUnderTest` sets defaults, so must override here:
        currentUser.IsAuthenticated.Returns(true);
        currentUser.IsMat.Returns(true);
        currentUser.UserOrganisationIsGroup.Returns(true);
        currentUser.UserOrganisationId.Returns(654321); // the ID for the group (MAT)
        currentUser.GroupSelectedSchoolUrn.Returns("123456");

        establishmentService.GetEstablishmentLinksWithRecommendationCounts(654321).Returns([]);

        var controller = new TestController();

        // Act
        var action = await sut.RouteBasedOnOrganisationTypeAsync(controller, page);

        // Assert
        var redirect = Assert.IsType<RedirectResult>(action);
        Assert.Equal(UrlConstants.SelectASchoolPage, redirect.Url);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationType_When_SchoolSelected_Then_ReturnsPageView()
    {
        // Arrange - A MAT user with a selected school, attempting to access the home page.
        // Home slug logic uses UrlConstants.HomePage.Replace("/", "")
        var homeSlug = UrlConstants.HomePage.Replace("/", ""); // usually ""
        var page = CreatePage(homeSlug, isLanding: false);

        var contentful = Substitute.For<IContentfulService>();
        var currentUser = Substitute.For<ICurrentUser>();
        var establishmentService = Substitute.For<IEstablishmentService>();

        // Setup establishmentService to return a list containing the selected school
        // (needed to verify that the user has access to the selected school)
        establishmentService
            .GetEstablishmentLinksWithRecommendationCounts(Arg.Any<int>())
            .Returns(
                new List<SqlEstablishmentLinkDto> { new SqlEstablishmentLinkDto { Urn = "123456" } }
            );

        var sut = CreateServiceUnderTest(
            contentful: contentful,
            currentUser: currentUser,
            establishmentService: establishmentService
        );

        // `CreateServiceUnderTest` sets defaults, so must override here:
        currentUser.IsAuthenticated.Returns(true);
        currentUser.IsMat.Returns(true);
        currentUser.GetActiveEstablishmentIdAsync().Returns(654321); // the ID for the group (MAT)
        currentUser.GroupSelectedSchoolUrn.Returns("123456");

        var controller = new TestController("/test-path");

        // Act
        var action = await sut.RouteBasedOnOrganisationTypeAsync(controller, page);

        // Assert
        var view = Assert.IsType<ViewResult>(action);
        Assert.Equal("Page", view.ViewName);
        var vm = Assert.IsType<PageViewModel>(view.Model);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationType_LandingPage_Path_Returns_CategoryLanding_View()
    {
        var categoryTitle = "Networking";
        var slug = categoryTitle.ToLower();
        var page = CreatePage(slug, isLanding: true);
        var category = CreateCategory(categoryTitle);

        var contentful = Substitute.For<IContentfulService>();
        contentful.GetCategoryBySlugAsync(slug, 4).Returns(category);

        var sut = CreateServiceUnderTest(contentful: contentful);
        var controller = new TestController();
        controller.TempData["SectionName"] = "Some Section";

        var action = await sut.RouteBasedOnOrganisationTypeAsync(controller, page);

        var view = Assert.IsType<ViewResult>(action);
        Assert.Equal(PagesViewBuilder.CategoryLandingPageView, view.ViewName);
        var vm = Assert.IsType<CategoryLandingPageViewModel>(view.Model);
        Assert.Equal(categoryTitle, vm.Title.Text);
        Assert.Equal(slug, vm.Slug);
        Assert.Equal("Some Section", vm.SectionName);

        await contentful.Received(1).GetCategoryBySlugAsync(slug, 4);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationType_LandingPage_Throws_When_Category_Not_Found()
    {
        var page = CreatePage("missing", isLanding: true);
        var contentful = Substitute.For<IContentfulService>();

        var sut = CreateServiceUnderTest(contentful: contentful);
        contentful.GetCategoryBySlugAsync("missing", 4).Returns((QuestionnaireCategoryEntry?)null);

        var controller = new TestController();

        await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            sut.RouteBasedOnOrganisationTypeAsync(controller, page)
        );
    }

    [Fact]
    public async Task RouteBasedOnOrganisationType_LandingPage_Throws_When_Category_Landing_Slug_Null()
    {
        var page = CreatePage(string.Empty, isLanding: true);
        var category = new QuestionnaireCategoryEntry
        {
            Header = new ComponentHeaderEntry { Text = "Missing" },
            LandingPage = null,
        };

        var contentful = Substitute.For<IContentfulService>();
        var sut = CreateServiceUnderTest(contentful: contentful);
        contentful.GetCategoryBySlugAsync(page.Slug, 4).Returns(category);

        var controller = new TestController();

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            sut.RouteBasedOnOrganisationTypeAsync(controller, page)
        );
    }

    [Fact]
    public async Task RouteBasedOnOrganisationType_NormalPage_Sets_Title_And_OrgName_When_Requested_And_Authenticated()
    {
        var page = CreatePage("about", isLanding: false, title: "About Us", displayOrg: true);

        var sut = CreateServiceUnderTest();
        var controller = new TestController();

        var action = await sut.RouteBasedOnOrganisationTypeAsync(controller, page);

        var view = Assert.IsType<ViewResult>(action);
        Assert.Equal("Page", view.ViewName);

        var vm = Assert.IsType<PageViewModel>(view.Model);
        Assert.Equal("Acme Academy", vm.ActiveEstablishmentName);
        Assert.Equal("123456", vm.ActiveEstablishmentUrn);
        Assert.Equal("About Us", controller.ViewData[StatePassingMechanismConstants.Title]); // processed title
        Assert.True(vm.DisplayBlueBanner); // default
    }

    [Fact]
    public async Task RouteBasedOnOrganisationType_NormalPage_NoTitle_Uses_Default_Page_Title()
    {
        var page = CreatePage("plain", isLanding: false, title: null, displayOrg: false);

        var sut = CreateServiceUnderTest();
        var controller = new TestController();

        var action = await sut.RouteBasedOnOrganisationTypeAsync(controller, page);

        var view = Assert.IsType<ViewResult>(action);
        Assert.Equal(
            PageTitleConstants.PlanTechnologyForYourSchool,
            controller.ViewData[StatePassingMechanismConstants.Title]
        );
    }

    [Fact]
    public async Task RouteBasedOnOrganisationType_InternalErrorPage_Disables_Blue_Banner()
    {
        var errors = ErrorOpts(internalId: "err-500");
        var page = CreatePage(
            "error",
            isLanding: false,
            title: "Err",
            displayOrg: false,
            id: "err-500"
        );

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
        var page = CreatePage("about", isLanding: false, title: "About", displayOrg: true);

        var current = Substitute.For<ICurrentUser>();
        var sut = CreateServiceUnderTest(currentUser: current);
        current.IsAuthenticated.Returns(false);
        current.IsMat.Returns(false);

        var controller = new TestController();

        var action = await sut.RouteBasedOnOrganisationTypeAsync(controller, page);

        var view = Assert.IsType<ViewResult>(action);
        var vm = Assert.IsType<PageViewModel>(view.Model);
        Assert.Null(vm.ActiveEstablishmentName);
        Assert.Null(vm.ActiveEstablishmentUrn);
    }

    // ---------- RouteToCategoryLandingPrintPageAsync ----------

    [Fact]
    public async Task RouteToCategoryLandingPrintPageAsync_RedirectsToHomePage_When_CategoryNotFound()
    {
        var contentful = Substitute.For<IContentfulService>();
        contentful
            .GetCategoryBySlugAsync("invalidCategory", 4)
            .Returns(default(QuestionnaireCategoryEntry?));

        var current = Substitute.For<ICurrentUser>();
        var sut = CreateServiceUnderTest(contentful: contentful, currentUser: current);
        current.IsAuthenticated.Returns(false);
        current.IsMat.Returns(false);

        var controller = new TestController();

        var slug = "categorySlug";
        var action = await sut.RouteToCategoryLandingPrintPageAsync(controller, slug);

        var redirect = Assert.IsType<RedirectToActionResult>(action);
        Assert.Equal(nameof(PagesController.GetByRoute), redirect.ActionName);
    }

    [Fact]
    public async Task RouteToCategoryLandingPrintPageAsync_When_CategoryFound_Then_ReturnsView()
    {
        var category = CreateCategory("Networking");
        var currentUser = Substitute.For<ICurrentUser>();
        var contentful = Substitute.For<IContentfulService>();
        var slug = "networking";
        var sut = CreateServiceUnderTest(contentful: contentful, currentUser: currentUser);

        currentUser.IsAuthenticated.Returns(true);
        currentUser.IsMat.Returns(true);
        currentUser.UserOrganisationId.Returns(654321); // the ID for the group (MAT)
        currentUser.GroupSelectedSchoolUrn.Returns("123456");

        contentful.GetCategoryBySlugAsync(slug).Returns(category);
        contentful.GetCategoryBySlugAsync(slug, 4).Returns(category);

        var controller = new TestController();

        var action = await sut.RouteToCategoryLandingPrintPageAsync(controller, slug);

        // Assert
        var view = Assert.IsType<ViewResult>(action);
        Assert.Equal(PagesViewBuilder.CategoryLandingPagePrintView, view.ViewName);
        var vm = Assert.IsType<CategoryLandingPageViewModel>(view.Model);
    }

    // ---------- RouteToShareStandardPageAsync ----------

    [Fact]
    public async Task RouteToShareStandardPageAsync_WhenInputModelIsNull_ReturnsShareView()
    {
        var controller = CreateController();
        var category = CreateCategory("Category");

        _contentfulService.GetCategoryBySlugAsync("category", 4).Returns(category);

        var sut = CreateServiceUnderTest(useCurrentUserDefaults: false);

        var result = await sut.RouteToShareStandardPageAsync(controller, "category", null);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("~/Views/Shared/Email/ShareByEmail.cshtml", viewResult.ViewName);

        var model = Assert.IsType<ShareByEmailViewModel>(viewResult.Model);
        Assert.Equal("Pages", model.PostController);
        Assert.Equal(nameof(PagesController.ShareStandard), model.PostAction);
        Assert.Equal("category", model.CategorySlug);
        Assert.Equal("Category", model.Caption);
        Assert.Equal("Share list of recommendations by email", model.Heading);
        Assert.Null(model.InputModel);
    }

    [Fact]
    public async Task RouteToShareStandardPageAsync_WhenModelStateIsInvalid_ReturnsShareViewWithInputModel()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("EmailAddresses", "Nope");

        var inputModel = new ShareByEmailInputViewModel
        {
            NameOfUser = "Drew",
            EmailAddresses = ["test@example.com"],
        };

        var category = CreateCategory("category");

        _contentfulService.GetCategoryBySlugAsync("category", 4).Returns(category);

        var sut = CreateServiceUnderTest(useCurrentUserDefaults: false);

        var result = await sut.RouteToShareStandardPageAsync(controller, "category", inputModel);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("~/Views/Shared/Email/ShareByEmail.cshtml", viewResult.ViewName);

        var model = Assert.IsType<ShareByEmailViewModel>(viewResult.Model);
        Assert.Same(inputModel, model.InputModel);
    }

    [Fact]
    public async Task RouteToShareStandardPageAsync_WhenCategoryNotFound_RedirectsToHome()
    {
        var controller = CreateController();

        _contentfulService
            .GetCategoryBySlugAsync("missing-category")
            .Returns((QuestionnaireCategoryEntry?)null);

        var sut = CreateServiceUnderTest(useCurrentUserDefaults: false);

        var result = await sut.RouteToShareStandardPageAsync(
            controller,
            "missing-category",
            new ShareByEmailInputViewModel()
        );

        var viewResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(PagesController).GetControllerNameSlug(), viewResult.ControllerName);
        Assert.NotNull(viewResult.RouteValues);
        Assert.Equal(UrlConstants.HomePage, viewResult.RouteValues["route"]);
    }

    [Fact]
    public async Task RouteToShareStandardPageAsync_WhenEstablishmentNameMissing_ThrowsInvalidDataException()
    {
        var controller = CreateController();
        var inputModel = CreateInputModel();

        var category = CreateCategory("category");
        var textBody = new ComponentTextBodyEntry
        {
            Sys = new SystemDetails { Id = "text-body-id" },
        };

        _contentfulService.GetCategoryBySlugAsync("category", 4).Returns(category);
        _contentfulService.GetTextBodyByIdAsync("text-body-id").Returns(textBody);
        _currentUser.GetActiveEstablishmentIdAsync().Returns(123);
        _currentUser.GetActiveEstablishmentNameAsync().Returns((string?)null);

        var sut = CreateServiceUnderTest(useCurrentUserDefaults: false);

        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            sut.RouteToShareStandardPageAsync(controller, "category", inputModel)
        );

        Assert.Equal(
            "Cannot send an email without an active establishment name",
            exception.Message
        );
    }

    [Fact]
    public async Task RouteToShareStandardPageAsync_WhenNotifySendSucceeds_RedirectsBackToSingleRecommendation()
    {
        var controller = CreateController();
        var inputModel = CreateInputModel();

        var category = CreateCategory("category");
        var textBody = new ComponentTextBodyEntry
        {
            Sys = new SystemDetails { Id = "text-body-id" },
        };

        _contentfulService.GetCategoryBySlugAsync("category", 4).Returns(category);
        _currentUser.GetActiveEstablishmentIdAsync().Returns(123);
        _currentUser.GetActiveEstablishmentNameAsync().Returns("Springfield Primary");

        _contentfulService.GetTextBodyByIdAsync("text-body-id").Returns(textBody);

        _notifyService
            .SendStandardEmail(
                Arg.Any<ShareByEmailModel>(),
                Arg.Any<List<QuestionnaireSectionEntry>>(),
                Arg.Any<List<SqlSectionStatusDto>>(),
                Arg.Any<Dictionary<string, SqlEstablishmentRecommendationHistoryDto>>(),
                Arg.Any<string>(),
                Arg.Any<string>()
            )
            .Returns([new NotifySendResult { Recipient = DefaultRecipient, Errors = [] }]);

        var sut = CreateServiceUnderTest(useCurrentUserDefaults: false);

        var result = await sut.RouteToShareStandardPageAsync(controller, "category", inputModel);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(PagesController.GetByRoute), redirect.ActionName);
        Assert.Equal(nameof(PagesController).GetControllerNameSlug(), redirect.ControllerName);

        var routeValues = Assert.IsType<IDictionary<string, object>>(
            redirect.RouteValues!,
            exactMatch: false
        );
        Assert.Equal("category", routeValues["route"]);

        _notifyService
            .SendStandardEmail(
                Arg.Any<ShareByEmailModel>(),
                Arg.Any<List<QuestionnaireSectionEntry>>(),
                Arg.Any<List<SqlSectionStatusDto>>(),
                Arg.Any<Dictionary<string, SqlEstablishmentRecommendationHistoryDto>>(),
                Arg.Any<string>(),
                Arg.Any<string>()
            )
            .Returns([new NotifySendResult { Recipient = DefaultRecipient, Errors = ["kaboom"] }]);

        _notifyService
            .Received(1)
            .SendStandardEmail(
                Arg.Is<ShareByEmailModel>(m =>
                    m.NameOfUser == inputModel.NameOfUser
                    && m.EmailAddresses.SequenceEqual(inputModel.EmailAddresses)
                    && m.UserMessage == inputModel.UserMessage
                ),
                Arg.Any<List<QuestionnaireSectionEntry>>(),
                Arg.Any<List<SqlSectionStatusDto>>(),
                Arg.Any<Dictionary<string, SqlEstablishmentRecommendationHistoryDto>>(),
                Arg.Any<string>(),
                Arg.Any<string>()
            );
    }

    [Fact]
    public async Task RouteToShareStandardPageAsync_WhenNotifySendHasErrors_RedirectsToNotifyError()
    {
        var controller = CreateController();
        var inputModel = CreateInputModel();

        var category = CreateCategory("category");
        var textBody = new ComponentTextBodyEntry
        {
            Sys = new SystemDetails { Id = "text-body-id" },
        };

        _contentfulService.GetCategoryBySlugAsync("category", 4).Returns(category);
        _currentUser.GetActiveEstablishmentIdAsync().Returns(123);
        _currentUser.GetActiveEstablishmentNameAsync().Returns("Springfield Primary");

        _contentfulService.GetTextBodyByIdAsync("text-body-id").Returns(textBody);

        _notifyService
            .SendStandardEmail(
                Arg.Any<ShareByEmailModel>(),
                Arg.Any<List<QuestionnaireSectionEntry>>(),
                Arg.Any<List<SqlSectionStatusDto>>(),
                Arg.Any<Dictionary<string, SqlEstablishmentRecommendationHistoryDto>>(),
                Arg.Any<string>(),
                Arg.Any<string>()
            )
            .Returns([new NotifySendResult { Recipient = DefaultRecipient, Errors = ["kaboom"] }]);

        var sut = CreateServiceUnderTest(useCurrentUserDefaults: false);

        var expected = PageRedirecter.RedirectToNotifyError(controller);

        var result = await sut.RouteToShareStandardPageAsync(controller, "category", inputModel);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(expected.ActionName, redirect.ActionName);
        Assert.Equal(expected.ControllerName, redirect.ControllerName);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationType_LandingPage_Path_Populates_RelatedActions_From_Page()
    {
        var categoryTitle = "Networking";
        var slug = categoryTitle.ToLower();
        var page = CreatePage(slug, isLanding: true);
        page.RelatedActions.Add(
            new RelatedActionEntry
            {
                Title = "Share this list of recommendations",
                Url = "/networking/share",
            }
        );
        page.RelatedActions.Add(
            new RelatedActionEntry
            {
                Title = "Print recommendations",
                Url = "/networking/print",
            }
        );

        var category = CreateCategory(categoryTitle);

        var contentful = Substitute.For<IContentfulService>();
        contentful.GetCategoryBySlugAsync(slug, 4).Returns(category);

        var sut = CreateServiceUnderTest(contentful: contentful);
        var controller = new TestController();
        controller.TempData["SectionName"] = "Some Section";

        var action = await sut.RouteBasedOnOrganisationTypeAsync(controller, page);

        var view = Assert.IsType<ViewResult>(action);
        Assert.Equal(PagesViewBuilder.CategoryLandingPageView, view.ViewName);
        var vm = Assert.IsType<CategoryLandingPageViewModel>(view.Model);
        Assert.Equal(2, vm.RelatedActions.Count);
        Assert.Equal("Share this list of recommendations", vm.RelatedActions[0].Text);
        Assert.Equal("/networking/share", vm.RelatedActions[0].Url);
        Assert.Equal("Print recommendations", vm.RelatedActions[1].Text);
        Assert.Equal("/networking/print", vm.RelatedActions[1].Url);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationType_LandingPage_Path_With_No_RelatedActions_Returns_Empty_RelatedActions()
    {
        var categoryTitle = "Networking";
        var slug = categoryTitle.ToLower();
        var page = CreatePage(slug, isLanding: true);
        var category = CreateCategory(categoryTitle);

        var contentful = Substitute.For<IContentfulService>();
        contentful.GetCategoryBySlugAsync(slug, 4).Returns(category);

        var sut = CreateServiceUnderTest(contentful: contentful);
        var controller = new TestController();
        controller.TempData["SectionName"] = "Some Section";

        var action = await sut.RouteBasedOnOrganisationTypeAsync(controller, page);

        var view = Assert.IsType<ViewResult>(action);
        Assert.Equal(PagesViewBuilder.CategoryLandingPageView, view.ViewName);
        var vm = Assert.IsType<CategoryLandingPageViewModel>(view.Model);
        Assert.Empty(vm.RelatedActions);
    }

    // ---------- BuildNotFoundViewModel ----------

    [Fact]
    public async Task BuildNotFoundViewModel_Uses_Contact_Link_From_Contentful()
    {
        var contentful = Substitute.For<IContentfulService>();
        contentful
            .GetLinkByIdAsync("contact-1")
            .Returns(new NavigationLinkEntry { Href = "/contact" });

        var sut = CreateServiceUnderTest(contentful: contentful);

        var vm = await sut.BuildNotFoundViewModelAsync();

        Assert.Equal("/contact", vm.ContactLinkHref);
        await contentful.Received(1).GetLinkByIdAsync("contact-1");
    }

    // ---------- BuildNotifyShareViewModel ----------

    // ---------- Support ----------

    private static ShareByEmailInputViewModel CreateInputModel()
    {
        return new ShareByEmailInputViewModel
        {
            NameOfUser = "Drew",
            EmailAddresses = ["drew@example.com"],
            UserMessage = "Hello",
        };
    }
}
