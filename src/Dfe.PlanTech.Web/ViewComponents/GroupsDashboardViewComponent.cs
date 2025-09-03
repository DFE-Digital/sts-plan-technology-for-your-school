using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

public class GroupsDashboardViewComponent(
    IGroupsDashboardViewComponentViewBuilder viewBuilder
) : ViewComponent
{
    private readonly IGroupsDashboardViewComponentViewBuilder _viewBuilder = viewBuilder ?? throw new ArgumentNullException(nameof(viewBuilder));

    public async Task<IViewComponentResult> InvokeAsync(QuestionnaireCategoryEntry category)
    {
        var viewModel = await _viewBuilder.BuildViewModelAsync(category);
        return View(viewModel);
    }
}
