using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Web.ViewBuilders;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.PagesViewBuilderTests;

public class RouteBasedOnOrganisationTypeAsyncTests : PagesViewBuilderTestBase
{
    private const string PageSlug = "page-slug";
    private const string PageTitle = "Page title";
    private const string TopicStartInternalName = "Digital leadership topic start";
    private const string CategoryHeader = "Networking";
    private const string CategorySlug = "networking";
    private const string SectionName = "Some section";

    [Fact]
    public async Task RouteBasedOnOrganisationTypeAsync_WhenCanSelectSchoolsAndNoFocusedSchool_RedirectsToSelectASchoolPage()
    {
        // Arrange
        var page = BuildPage();
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.IsMat.Returns(true);
        _currentUser.CanSelectSchools.Returns(true);
        _currentUser.GetFocusedEstablishmentAsync().Returns((SelectedEstablishmentDataModel?)null);
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBasedOnOrganisationTypeAsync(controller, page);

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal(UrlConstants.SelectASchoolPage, redirect.Url);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationTypeAsync_WhenCanSelectSchoolsAndFocusedSchoolIsSet_ReturnsPageView()
    {
        // Arrange
        var page = BuildPage();
        var focusedSchool = new SelectedEstablishmentDataModelBuilder().Build();
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.IsMat.Returns(true);
        _currentUser.CanSelectSchools.Returns(true);
        _currentUser.GetFocusedEstablishmentAsync().Returns(focusedSchool);
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBasedOnOrganisationTypeAsync(controller, page);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Page", view.ViewName);
        var viewModel = Assert.IsType<PageViewModel>(view.Model);
        Assert.Same(page, viewModel.Page);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationTypeAsync_WhenUserCannotSelectSchools_ReturnsPageView()
    {
        // Arrange
        var page = BuildPage();
        var focusedSchool = new SelectedEstablishmentDataModelBuilder().Build();
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.IsSchool.Returns(true);
        _currentUser.CanSelectSchools.Returns(false);
        _currentUser.GetFocusedEstablishmentAsync().Returns(focusedSchool);
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBasedOnOrganisationTypeAsync(controller, page);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Page", view.ViewName);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationTypeAsync_WhenUserIsNotAuthenticated_ReturnsPageView()
    {
        // Arrange
        var page = BuildPage();
        _currentUser.IsAuthenticated.Returns(false);
        _currentUser.CanSelectSchools.Returns(false);
        _currentUser.GetFocusedEstablishmentAsync().Returns((SelectedEstablishmentDataModel?)null);
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBasedOnOrganisationTypeAsync(controller, page);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Page", view.ViewName);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationTypeAsync_WhenPageDoesNotRequireAuthorisationAndNoFocusedSchool_ReturnsPageView()
    {
        // Arrange
        var page = BuildPage(requiresAuthorisation: false);
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.IsMat.Returns(true);
        _currentUser.CanSelectSchools.Returns(true);
        _currentUser.GetFocusedEstablishmentAsync().Returns((SelectedEstablishmentDataModel?)null);
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBasedOnOrganisationTypeAsync(controller, page);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Page", view.ViewName);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationTypeAsync_WhenMatTopicStartPageAndNoFocusedSchool_ReturnsPageViewWithTrustSchoolAssessmentTable()
    {
        // Arrange
        var page = BuildPage(internalName: TopicStartInternalName);
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.IsMat.Returns(true);
        _currentUser.CanSelectSchools.Returns(true);
        _currentUser.GetFocusedEstablishmentAsync().Returns((SelectedEstablishmentDataModel?)null);
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBasedOnOrganisationTypeAsync(controller, page);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Page", view.ViewName);
        var viewModel = Assert.IsType<PageViewModel>(view.Model);
        Assert.True(viewModel.ShowTrustSchoolAssessmentTable);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationTypeAsync_WhenNonMatUserCanSelectSchoolsOnTopicStartPageWithoutFocusedSchool_RedirectsToSelectASchoolPage()
    {
        // Arrange
        var page = BuildPage(internalName: TopicStartInternalName);
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.IsMat.Returns(false);
        _currentUser.CanSelectSchools.Returns(true);
        _currentUser.GetFocusedEstablishmentAsync().Returns((SelectedEstablishmentDataModel?)null);
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBasedOnOrganisationTypeAsync(controller, page);

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal(UrlConstants.SelectASchoolPage, redirect.Url);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationTypeAsync_WhenMatUserOnTopicStartPageWithFocusedSchool_ShowsTrustSchoolAssessmentTable()
    {
        // Arrange
        var page = BuildPage(internalName: TopicStartInternalName);
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.IsMat.Returns(true);
        _currentUser.CanSelectSchools.Returns(true);
        _currentUser
            .GetFocusedEstablishmentAsync()
            .Returns(new SelectedEstablishmentDataModelBuilder().Build());
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBasedOnOrganisationTypeAsync(controller, page);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<PageViewModel>(view.Model);
        Assert.True(viewModel.ShowTrustSchoolAssessmentTable);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationTypeAsync_WhenMatUserOnNonTopicStartPage_DoesNotShowTrustSchoolAssessmentTable()
    {
        // Arrange
        var page = BuildPage(internalName: "Digital leadership overview");
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.IsMat.Returns(true);
        _currentUser.CanSelectSchools.Returns(true);
        _currentUser
            .GetFocusedEstablishmentAsync()
            .Returns(new SelectedEstablishmentDataModelBuilder().Build());
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBasedOnOrganisationTypeAsync(controller, page);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<PageViewModel>(view.Model);
        Assert.False(viewModel.ShowTrustSchoolAssessmentTable);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationTypeAsync_WhenSchoolUserOnTopicStartPage_DoesNotShowTrustSchoolAssessmentTable()
    {
        // Arrange
        var page = BuildPage(internalName: TopicStartInternalName);
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.IsMat.Returns(false);
        _currentUser.CanSelectSchools.Returns(false);
        _currentUser
            .GetFocusedEstablishmentAsync()
            .Returns(new SelectedEstablishmentDataModelBuilder().Build());
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBasedOnOrganisationTypeAsync(controller, page);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<PageViewModel>(view.Model);
        Assert.False(viewModel.ShowTrustSchoolAssessmentTable);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationTypeAsync_WhenLandingPageRequiresSchoolSelection_RedirectsToSelectASchoolPage()
    {
        // Arrange
        var page = BuildPage(isLandingPage: true);
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.IsMat.Returns(true);
        _currentUser.CanSelectSchools.Returns(true);
        _currentUser.GetFocusedEstablishmentAsync().Returns((SelectedEstablishmentDataModel?)null);
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBasedOnOrganisationTypeAsync(controller, page);

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal(UrlConstants.SelectASchoolPage, redirect.Url);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationTypeAsync_WhenPageIsLandingPage_ReturnsCategoryLandingView()
    {
        // Arrange
        var page = BuildPage(slug: CategorySlug, isLandingPage: true);
        var category = BuildCategory();
        _contentfulService.GetCategoryBySlugAsync(CategorySlug, 4).Returns(category);
        var controller = new TestController();
        controller.TempData[StatePassingMechanismConstants.SectionName] = SectionName;
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBasedOnOrganisationTypeAsync(controller, page);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(PagesViewBuilder.CategoryLandingPageView, view.ViewName);
        var viewModel = Assert.IsType<CategoryLandingPageViewModel>(view.Model);
        Assert.Equal(CategoryHeader, viewModel.Title.Text);
        Assert.Equal(CategorySlug, viewModel.Slug);
        Assert.Same(category, viewModel.Category);
        Assert.Equal(SectionName, viewModel.SectionName);
        Assert.Empty(viewModel.RelatedActions);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationTypeAsync_WhenLandingPageHasRelatedActions_ReturnsViewWithRelatedActions()
    {
        // Arrange
        var page = BuildPage(slug: CategorySlug, isLandingPage: true);
        page.RelatedActions.Add(
            new RelatedActionEntry { Title = "Share recommendations", Url = "/networking/share" }
        );
        page.RelatedActions.Add(
            new RelatedActionEntry { Title = "Print recommendations", Url = "/networking/print" }
        );
        _contentfulService.GetCategoryBySlugAsync(CategorySlug, 4).Returns(BuildCategory());
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBasedOnOrganisationTypeAsync(controller, page);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<CategoryLandingPageViewModel>(view.Model);
        Assert.Collection(
            viewModel.RelatedActions,
            relatedAction =>
            {
                Assert.Equal("Share recommendations", relatedAction.Text);
                Assert.Equal("/networking/share", relatedAction.Url);
            },
            relatedAction =>
            {
                Assert.Equal("Print recommendations", relatedAction.Text);
                Assert.Equal("/networking/print", relatedAction.Url);
            }
        );
    }

    [Fact]
    public async Task RouteBasedOnOrganisationTypeAsync_WhenRelatedActionIsMissingTitleOrUrl_ExcludesIncompleteRelatedActions()
    {
        // Arrange
        var page = BuildPage(slug: CategorySlug, isLandingPage: true);
        page.RelatedActions.Add(
            new RelatedActionEntry { Title = "Share recommendations", Url = "/networking/share" }
        );
        page.RelatedActions.Add(new RelatedActionEntry { Title = "No url", Url = null });
        page.RelatedActions.Add(new RelatedActionEntry { Title = " ", Url = "/networking/blank" });
        _contentfulService.GetCategoryBySlugAsync(CategorySlug, 4).Returns(BuildCategory());
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBasedOnOrganisationTypeAsync(controller, page);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<CategoryLandingPageViewModel>(view.Model);
        var relatedAction = Assert.Single(viewModel.RelatedActions);
        Assert.Equal("Share recommendations", relatedAction.Text);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationTypeAsync_WhenLandingPageCategoryIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        var page = BuildPage(slug: "missing-category", isLandingPage: true);
        _contentfulService
            .GetCategoryBySlugAsync("missing-category", 4)
            .Returns((QuestionnaireCategoryEntry?)null);
        var controller = new TestController("/missing-category");
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteBasedOnOrganisationTypeAsync(controller, page)
        );
    }

    [Fact]
    public async Task RouteBasedOnOrganisationTypeAsync_WhenLandingPageSlugIsEmpty_ThrowsInvalidDataException()
    {
        // Arrange
        var page = BuildPage(slug: string.Empty, isLandingPage: true);
        _contentfulService.GetCategoryBySlugAsync(string.Empty, 4).Returns(BuildCategory());
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            systemUnderTest.RouteBasedOnOrganisationTypeAsync(controller, page)
        );
        Assert.Equal("Page Slug cannot be empty", exception.Message);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationTypeAsync_WhenPageHasTitle_SetsDecodedTitleWithNonBreakingHyphens()
    {
        // Arrange
        var page = BuildPage(title: "Self-assessment &amp; planning");
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RouteBasedOnOrganisationTypeAsync(controller, page);

        // Assert
        Assert.Equal(
            "Self\u2011assessment & planning",
            controller.ViewData[StatePassingMechanismConstants.Title]
        );
    }

    [Fact]
    public async Task RouteBasedOnOrganisationTypeAsync_WhenPageHasNoTitle_SetsDefaultTitle()
    {
        // Arrange
        var page = BuildPage(title: null);
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RouteBasedOnOrganisationTypeAsync(controller, page);

        // Assert
        Assert.Equal(
            PageTitleConstants.PlanTechnologyForYourSchool,
            controller.ViewData[StatePassingMechanismConstants.Title]
        );
    }

    [Fact]
    public async Task RouteBasedOnOrganisationTypeAsync_WhenPageIsInternalErrorPage_DisablesBlueBanner()
    {
        // Arrange
        var page = BuildPage(id: InternalErrorPageId);
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBasedOnOrganisationTypeAsync(controller, page);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<PageViewModel>(view.Model);
        Assert.False(viewModel.DisplayBlueBanner);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationTypeAsync_WhenPageIsNotInternalErrorPage_DisplaysBlueBanner()
    {
        // Arrange
        var page = BuildPage();
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBasedOnOrganisationTypeAsync(controller, page);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<PageViewModel>(view.Model);
        Assert.True(viewModel.DisplayBlueBanner);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationTypeAsync_WhenDisplayOrganisationNameAndFocusedSchoolIsSet_SetsActiveEstablishmentOnViewModel()
    {
        // Arrange
        var page = BuildPage(displayOrganisationName: true);
        var focusedSchool = new SelectedEstablishmentDataModelBuilder()
            .WithName("Acme Academy")
            .WithUrn("123456")
            .Build();
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.GetFocusedEstablishmentAsync().Returns(focusedSchool);
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBasedOnOrganisationTypeAsync(controller, page);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<PageViewModel>(view.Model);
        Assert.Equal("Acme Academy", viewModel.ActiveEstablishmentName);
        Assert.Equal("123456", viewModel.ActiveEstablishmentUrn);
        Assert.Empty(_logger.Collector.GetSnapshot());
    }

    [Fact]
    public async Task RouteBasedOnOrganisationTypeAsync_WhenDisplayOrganisationNameIsFalse_DoesNotSetActiveEstablishmentOnViewModel()
    {
        // Arrange
        var page = BuildPage(displayOrganisationName: false);
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser
            .GetFocusedEstablishmentAsync()
            .Returns(new SelectedEstablishmentDataModelBuilder().Build());
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBasedOnOrganisationTypeAsync(controller, page);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<PageViewModel>(view.Model);
        Assert.Null(viewModel.ActiveEstablishmentName);
        Assert.Null(viewModel.ActiveEstablishmentUrn);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationTypeAsync_WhenDisplayOrganisationNameAndUserIsNotAuthenticated_LogsWarningAndDoesNotSetActiveEstablishment()
    {
        // Arrange
        var page = BuildPage(displayOrganisationName: true);
        _currentUser.IsAuthenticated.Returns(false);
        _currentUser.GetFocusedEstablishmentAsync().Returns((SelectedEstablishmentDataModel?)null);
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBasedOnOrganisationTypeAsync(controller, page);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<PageViewModel>(view.Model);
        Assert.Null(viewModel.ActiveEstablishmentName);
        Assert.Null(viewModel.ActiveEstablishmentUrn);
        var logRecord = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Warning, logRecord.Level);
        Assert.Contains("user is not authenticated", logRecord.Message);
        Assert.Contains(PageTitle, logRecord.Message);
    }

    [Fact]
    public async Task RouteBasedOnOrganisationTypeAsync_WhenDisplayOrganisationNameAndNoFocusedSchool_LogsWarningAndDoesNotSetActiveEstablishment()
    {
        // Arrange
        var page = BuildPage(displayOrganisationName: true, requiresAuthorisation: false);
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.CanSelectSchools.Returns(true);
        _currentUser.GetFocusedEstablishmentAsync().Returns((SelectedEstablishmentDataModel?)null);
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBasedOnOrganisationTypeAsync(controller, page);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<PageViewModel>(view.Model);
        Assert.Null(viewModel.ActiveEstablishmentName);
        Assert.Null(viewModel.ActiveEstablishmentUrn);
        var logRecord = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Warning, logRecord.Level);
        Assert.Contains("does not have a focused school", logRecord.Message);
    }

    private static PageEntry BuildPage(
        string slug = PageSlug,
        string id = "page-id",
        string? title = PageTitle,
        string? internalName = null,
        bool isLandingPage = false,
        bool requiresAuthorisation = true,
        bool displayOrganisationName = false
    ) =>
        new()
        {
            Sys = new SystemDetails(id),
            Slug = slug,
            InternalName = internalName!,
            Title = title is null ? null : new ComponentTitleEntry(title),
            IsLandingPage = isLandingPage,
            RequiresAuthorisation = requiresAuthorisation,
            DisplayOrganisationName = displayOrganisationName,
        };

    private static QuestionnaireCategoryEntry BuildCategory() =>
        new() { Header = new ComponentHeaderEntry { Text = CategoryHeader } };
}
