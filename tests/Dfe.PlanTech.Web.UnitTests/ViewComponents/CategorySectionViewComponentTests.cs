using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewComponents;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using NSubstitute;

namespace Dfe.PlanTech.Web.Tests.ViewComponents
{
    public class CategorySectionViewComponentTests
    {
        [Fact]
        public async Task InvokeAsync_ShouldReturnViewWithViewModel()
        {
            var mockViewBuilder = Substitute.For<ICategorySectionViewComponentViewBuilder>();
            var categoryEntry = new QuestionnaireCategoryEntry();
            var expectedViewModel = new CategoryCardsViewComponentViewModel()
            {
                CategoryHeaderText = "Arbitrary header text",
                Description = new MissingComponentEntry(),
            };

            mockViewBuilder
                .BuildViewModelAsync(categoryEntry, CategoryLandingContext.School)
                .Returns(Task.FromResult(expectedViewModel));

            var viewComponent = new CategorySectionViewComponent(mockViewBuilder);

            var result = await viewComponent.InvokeAsync(categoryEntry);

            var viewResult = Assert.IsType<ViewViewComponentResult>(result);
            Assert.NotNull(viewResult.ViewData);
            Assert.Equal(expectedViewModel, viewResult.ViewData.Model);

            await mockViewBuilder
                .Received(1)
                .BuildViewModelAsync(categoryEntry, CategoryLandingContext.School);
        }

        [Fact]
        public async Task InvokeAsync_WithMatContext_ShouldPassMatContextToViewBuilder()
        {
            var mockViewBuilder = Substitute.For<ICategorySectionViewComponentViewBuilder>();
            var categoryEntry = new QuestionnaireCategoryEntry();
            var expectedViewModel = new CategoryCardsViewComponentViewModel()
            {
                CategoryHeaderText = "Arbitrary header text",
                Description = new MissingComponentEntry(),
                Context = CategoryLandingContext.MAT,
            };

            mockViewBuilder
                .BuildViewModelAsync(categoryEntry, CategoryLandingContext.MAT)
                .Returns(Task.FromResult(expectedViewModel));

            var viewComponent = new CategorySectionViewComponent(mockViewBuilder);

            var result = await viewComponent.InvokeAsync(
                categoryEntry,
                CategoryLandingContext.MAT
            );

            var viewResult = Assert.IsType<ViewViewComponentResult>(result);
            Assert.NotNull(viewResult.ViewData);
            Assert.Equal(expectedViewModel, viewResult.ViewData.Model);

            await mockViewBuilder
                .Received(1)
                .BuildViewModelAsync(categoryEntry, CategoryLandingContext.MAT);
        }
    }
}
