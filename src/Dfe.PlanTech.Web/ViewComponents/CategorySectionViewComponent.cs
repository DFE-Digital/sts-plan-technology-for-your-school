using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Web.ViewBuilders;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

public class CategorySectionViewComponent(
    ILogger<CategorySectionViewComponent> logger,
    CategorySectionViewComponentViewBuilder viewBuilder
) : ViewComponent
{
    private readonly ILogger<CategorySectionViewComponent> _logger = logger;
    private readonly CategorySectionViewComponentViewBuilder _viewBuilder = viewBuilder ?? throw new ArgumentNullException(nameof(viewBuilder));

    public async Task<IViewComponentResult> InvokeAsync(CmsCategoryDto category)
    {
        var viewModel = await _viewBuilder.BuildViewModelAsync(category);
        return View(viewModel);
    }
}
