using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

public class CategorySectionViewComponent(ICategorySectionViewComponentViewBuilder viewBuilder)
    : ViewComponent
{
    private readonly ICategorySectionViewComponentViewBuilder _viewBuilder =
        viewBuilder ?? throw new ArgumentNullException(nameof(viewBuilder));

    public async Task<IViewComponentResult> InvokeAsync(
        QuestionnaireCategoryEntry category,
        CategoryLandingContext context = CategoryLandingContext.School
    )
    {
        var viewModel = await _viewBuilder.BuildViewModelAsync(
            category,
            context
        );

        return View(viewModel);
    }
}
