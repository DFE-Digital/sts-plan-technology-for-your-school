using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Web.ViewBuilders;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

public class CategoryLandingViewComponent(
    CategoryLandingViewComponentViewBuilder viewBuilder
) : ViewComponent
{
    private readonly CategoryLandingViewComponentViewBuilder _viewBuilder = viewBuilder ?? throw new ArgumentNullException(nameof(viewBuilder));

    public async Task<IViewComponentResult> InvokeAsync(CmsQuestionnaireCategoryDto category, string slug)
    {
        var viewModel = await _viewBuilder.BuildViewModelAsync(category, slug);
        return View(viewModel);
    }
}
