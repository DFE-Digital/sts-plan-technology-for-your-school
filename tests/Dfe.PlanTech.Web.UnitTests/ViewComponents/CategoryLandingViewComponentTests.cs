using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewComponents;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewComponents;

public class CategoryLandingViewComponentTests
{
    [Fact]
    public async Task InvokeAsync_ShouldCallBuildViewModelAsync_WithCorrectParameters()
    {
        var viewBuilder = Substitute.For<ICategoryLandingViewComponentViewBuilder>();
        var component = new CategoryLandingViewComponent(viewBuilder);

        var category = new QuestionnaireCategoryEntry
        {
            Sections = new List<QuestionnaireSectionEntry> { new() },
        };
        var slug = "test-slug";
        var sectionName = "test-section";

        var expectedViewModel = new CategoryLandingViewComponentViewModel
        {
            CompletedSectionsCount = 1,
            CategoryName = "Test Category",
            CategorySlug = slug,
            SectionName = sectionName,
            CategoryLandingSections = new List<CategoryLandingSectionViewModel>(),
            Sections = category.Sections,
            Print = false,
            StatusLinkPartialName = "status",
        };

        viewBuilder
            .BuildViewModelAsync(
                category,
                slug,
                sectionName,
                RecommendationConstants.DefaultSortOrder,
                false,
                CategoryLandingContext.School
            )
            .Returns(Task.FromResult(expectedViewModel));

        var result = await component.InvokeAsync(
            category,
            slug,
            sectionName,
            RecommendationConstants.DefaultSortOrder
        );

        await viewBuilder
            .Received(1)
            .BuildViewModelAsync(
                category,
                slug,
                sectionName,
                RecommendationSortOrder.Default.ToString(),
                false,
                CategoryLandingContext.School
            );
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        Assert.NotNull(viewResult.ViewData);
        Assert.Equal(expectedViewModel, viewResult.ViewData.Model);
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturnViewResult_WithExpectedModel()
    {
        var viewBuilder = Substitute.For<ICategoryLandingViewComponentViewBuilder>();
        var component = new CategoryLandingViewComponent(viewBuilder);

        var category = new QuestionnaireCategoryEntry
        {
            Sections = new List<QuestionnaireSectionEntry> { new() },
        };
        var slug = "another-slug";
        string? sectionName = null;

        var expectedViewModel = new CategoryLandingViewComponentViewModel
        {
            CompletedSectionsCount = 0,
            CategoryName = "Another Category",
            CategorySlug = slug,
            SectionName = sectionName,
            CategoryLandingSections = new List<CategoryLandingSectionViewModel>(),
            Sections = category.Sections,
            Print = false,
            StatusLinkPartialName = "status",
        };

        viewBuilder
            .BuildViewModelAsync(
                category,
                slug,
                sectionName,
                RecommendationConstants.DefaultSortOrder,
                false,
                CategoryLandingContext.School
            )
            .Returns(Task.FromResult(expectedViewModel));

        var result = await component.InvokeAsync(
            category,
            slug,
            sectionName,
            RecommendationConstants.DefaultSortOrder
        );

        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        Assert.NotNull(viewResult.ViewData);
        Assert.Equal(expectedViewModel, viewResult.ViewData.Model);
    }

    [Fact]
    public async Task InvokeAsync_ShouldPassMatContextToBuildViewModelAsync()
    {
        var viewBuilder = Substitute.For<ICategoryLandingViewComponentViewBuilder>();
        var component = new CategoryLandingViewComponent(viewBuilder);

        var category = new QuestionnaireCategoryEntry
        {
            Sections = new List<QuestionnaireSectionEntry> { new() },
        };
        var slug = "test-slug";
        var sectionName = "test-section";

        var expectedViewModel = new CategoryLandingViewComponentViewModel
        {
            CategoryName = "Test Category",
            CategorySlug = slug,
            SectionName = sectionName,
            CategoryLandingSections = new List<CategoryLandingSectionViewModel>(),
            Sections = category.Sections,
            StatusLinkPartialName = "status",
            Context = CategoryLandingContext.MAT,
        };

        viewBuilder
            .BuildViewModelAsync(
                category,
                slug,
                sectionName,
                RecommendationConstants.DefaultSortOrder,
                false,
                CategoryLandingContext.MAT
            )
            .Returns(Task.FromResult(expectedViewModel));

        var result = await component.InvokeAsync(
            category,
            slug,
            sectionName,
            RecommendationConstants.DefaultSortOrder,
            false,
            CategoryLandingContext.MAT
        );

        await viewBuilder
            .Received(1)
            .BuildViewModelAsync(
                category,
                slug,
                sectionName,
                RecommendationConstants.DefaultSortOrder,
                false,
                CategoryLandingContext.MAT
            );

        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        Assert.NotNull(viewResult.ViewData);
        Assert.Equal(expectedViewModel, viewResult.ViewData.Model);
    }
}
