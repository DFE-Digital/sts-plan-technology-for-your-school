using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Web.ViewBuilders;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

public class CategoryLandingViewComponent(
    CategoryLandingViewComponentViewBuilder viewBuilder
) : ViewComponent
{
    private readonly CategoryLandingViewComponentViewBuilder _viewBuilder = viewBuilder ?? throw new ArgumentNullException(nameof(viewBuilder));

    public async Task<IViewComponentResult> InvokeAsync(QuestionnaireCategoryEntry category, string slug, string? sectionName)
    {
        var viewModel = await _viewBuilder.BuildViewModelAsync(category, slug, sectionName);
        return View(viewModel);
    }
}
