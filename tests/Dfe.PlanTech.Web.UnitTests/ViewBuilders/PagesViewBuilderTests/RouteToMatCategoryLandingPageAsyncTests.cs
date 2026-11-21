using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.ViewBuilders;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.PagesViewBuilderTests;

public class RouteToMatCategoryLandingPageAsyncTests : PagesViewBuilderTestBase
{
    private const string CategorySlug = "leadership-governance-standard";
    private const string CategoryHeader = "Leadership and governance";

    [Fact]
    public async Task RouteToMatCategoryLandingPageAsync_WhenCategoryIsNotFound_RedirectsToHomePage()
    {
        // Arrange
        _contentfulService
            .GetCategoryBySlugAsync("missing-category", 4)
            .Returns((QuestionnaireCategoryEntry?)null);
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToMatCategoryLandingPageAsync(
            controller,
            "missing-category"
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(PagesController.GetByRoute), redirect.ActionName);
        Assert.Equal(nameof(PagesController).GetControllerNameSlug(), redirect.ControllerName);
        Assert.Equal(UrlConstants.HomePage, redirect.RouteValues!["route"]);
    }

    [Fact]
    public async Task RouteToMatCategoryLandingPageAsync_WhenCategoryAndPageAreFound_ReturnsMatViewWithRelatedActions()
    {
        // Arrange
        var category = new QuestionnaireCategoryEntry
        {
            Header = new ComponentHeaderEntry { Text = CategoryHeader },
        };
        var page = new PageEntry
        {
            Sys = new SystemDetails("page-id"),
            Slug = CategorySlug,
            RelatedActions =
            [
                new RelatedActionEntry
                {
                    Title = "Share recommendations",
                    Url = "/leadership-governance-standard/share",
                },
                new RelatedActionEntry
                {
                    Title = "Print recommendations",
                    Url = "/leadership-governance-standard/print",
                },
            ],
        };
        _contentfulService.GetCategoryBySlugAsync(CategorySlug, 4).Returns(category);
        _contentfulService.GetPageBySlugAsync(CategorySlug).Returns(page);
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToMatCategoryLandingPageAsync(
            controller,
            CategorySlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(PagesViewBuilder.CategoryLandingPageView, view.ViewName);
        var viewModel = Assert.IsType<CategoryLandingPageViewModel>(view.Model);
        Assert.Equal(CategoryLandingContext.MAT, viewModel.Context);
        Assert.Equal(CategorySlug, viewModel.Slug);
        Assert.Collection(
            viewModel.RelatedActions,
            relatedAction =>
            {
                Assert.Equal("Share recommendations", relatedAction.Text);
                Assert.Equal("/leadership-governance-standard/share", relatedAction.Url);
            },
            relatedAction =>
            {
                Assert.Equal("Print recommendations", relatedAction.Text);
                Assert.Equal("/leadership-governance-standard/print", relatedAction.Url);
            }
        );
    }

    [Fact]
    public async Task RouteToMatCategoryLandingPageAsync_WhenCategoryIsFound_SetsTitleToCategoryHeader()
    {
        // Arrange
        var category = new QuestionnaireCategoryEntry
        {
            Header = new ComponentHeaderEntry { Text = CategoryHeader },
        };
        _contentfulService.GetCategoryBySlugAsync(CategorySlug, 4).Returns(category);
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RouteToMatCategoryLandingPageAsync(controller, CategorySlug);

        // Assert
        Assert.Equal(CategoryHeader, controller.ViewData[StatePassingMechanismConstants.Title]);
    }

    [Fact]
    public async Task RouteToMatCategoryLandingPageAsync_WhenPageIsNotFound_ReturnsMatViewWithoutRelatedActions()
    {
        // Arrange
        var category = new QuestionnaireCategoryEntry
        {
            Header = new ComponentHeaderEntry { Text = CategoryHeader },
        };
        _contentfulService.GetCategoryBySlugAsync(CategorySlug, 4).Returns(category);
        _contentfulService.GetPageBySlugAsync(CategorySlug).Returns((PageEntry)null!);
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToMatCategoryLandingPageAsync(
            controller,
            CategorySlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<CategoryLandingPageViewModel>(view.Model);
        Assert.Empty(viewModel.RelatedActions);
    }
}
