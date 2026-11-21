using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.ViewBuilders;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.PagesViewBuilderTests;

public class RouteToCategoryLandingPrintPageAsyncTests : PagesViewBuilderTestBase
{
    private const string CategorySlug = "networking";
    private const string SectionName = "Some section";

    [Fact]
    public async Task RouteToCategoryLandingPrintPageAsync_WhenCategoryIsNotFound_RedirectsToHomePage()
    {
        // Arrange
        _contentfulService
            .GetCategoryBySlugAsync("missing-category", 4)
            .Returns((QuestionnaireCategoryEntry?)null);
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToCategoryLandingPrintPageAsync(
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
    public async Task RouteToCategoryLandingPrintPageAsync_WhenCategoryIsFound_ReturnsPrintViewWithSchoolContext()
    {
        // Arrange
        var category = new QuestionnaireCategoryEntry
        {
            Header = new ComponentHeaderEntry { Text = "Networking" },
        };
        _contentfulService.GetCategoryBySlugAsync(CategorySlug, 4).Returns(category);
        var controller = new TestController();
        controller.TempData[StatePassingMechanismConstants.SectionName] = SectionName;
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToCategoryLandingPrintPageAsync(
            controller,
            CategorySlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(PagesViewBuilder.CategoryLandingPagePrintView, view.ViewName);
        var viewModel = Assert.IsType<CategoryLandingPageViewModel>(view.Model);
        Assert.Equal(CategorySlug, viewModel.Slug);
        Assert.Equal("Networking", viewModel.Title.Text);
        Assert.Same(category, viewModel.Category);
        Assert.Equal(SectionName, viewModel.SectionName);
        Assert.Equal(CategoryLandingContext.School, viewModel.Context);
        Assert.Empty(viewModel.RelatedActions);
    }

    [Fact]
    public async Task RouteToCategoryLandingPrintPageAsync_WhenContextIsMat_ReturnsPrintViewWithMatContext()
    {
        // Arrange
        var category = new QuestionnaireCategoryEntry
        {
            Header = new ComponentHeaderEntry { Text = "Networking" },
        };
        _contentfulService.GetCategoryBySlugAsync(CategorySlug, 4).Returns(category);
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToCategoryLandingPrintPageAsync(
            controller,
            CategorySlug,
            CategoryLandingContext.MAT
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(PagesViewBuilder.CategoryLandingPagePrintView, view.ViewName);
        var viewModel = Assert.IsType<CategoryLandingPageViewModel>(view.Model);
        Assert.Equal(CategoryLandingContext.MAT, viewModel.Context);
    }
}
