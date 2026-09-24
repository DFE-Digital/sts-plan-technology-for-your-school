using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

public class CategoryLandingViewComponent(ICategoryLandingViewComponentViewBuilder viewBuilder)
    : ViewComponent
{
    private readonly ICategoryLandingViewComponentViewBuilder _viewBuilder =
        viewBuilder ?? throw new ArgumentNullException(nameof(viewBuilder));

    public async Task<IViewComponentResult> InvokeAsync(
        QuestionnaireCategoryEntry category,
        string slug,
        string? sectionName,
        string? sortOrder,
        bool print = false,
        CategoryLandingContext context = CategoryLandingContext.School
    )
    {
        var viewModel = await _viewBuilder.BuildViewModelAsync(
            category,
            slug,
            sectionName,
            sortOrder,
            print,
            context
        );

        return View(viewModel);
    }
}
