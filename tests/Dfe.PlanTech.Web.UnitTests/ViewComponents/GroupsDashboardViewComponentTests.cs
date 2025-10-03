using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewComponents;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using NSubstitute;

namespace Dfe.PlanTech.Web.Tests.ViewComponents
{
    public class GroupsDashboardViewComponentTests
    {
        [Fact]
        public async Task InvokeAsync_ShouldReturnViewWithViewModel()
        {
            var mockViewBuilder = Substitute.For<IGroupsDashboardViewComponentViewBuilder>();
            var categoryEntry = new QuestionnaireCategoryEntry();
            var expectedViewModel = new GroupsDashboardViewComponentViewModel();

            mockViewBuilder.BuildViewModelAsync(categoryEntry).Returns(Task.FromResult(expectedViewModel));

            var viewComponent = new GroupsDashboardViewComponent(mockViewBuilder);

            var result = await viewComponent.InvokeAsync(categoryEntry);

            var viewResult = Assert.IsType<ViewViewComponentResult>(result);
            Assert.NotNull(viewResult.ViewData);
            Assert.Equal(expectedViewModel, viewResult.ViewData.Model);

            await mockViewBuilder.Received(1).BuildViewModelAsync(categoryEntry);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenViewBuilderIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new GroupsDashboardViewComponent(null));
            Assert.Equal("viewBuilder", exception.ParamName);
        }
    }
}
