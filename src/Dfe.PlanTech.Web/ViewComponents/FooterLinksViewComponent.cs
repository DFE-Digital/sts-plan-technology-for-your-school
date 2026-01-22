using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

/// <summary>
/// View component that retrieves and displays links in the layout's footer
/// </summary>
public class FooterLinksViewComponent(IFooterLinksViewComponentViewBuilder viewBuilder)
    : ViewComponent
{
    private readonly IFooterLinksViewComponentViewBuilder _viewBuilder =
        viewBuilder ?? throw new ArgumentNullException(nameof(viewBuilder));

    /// <summary>
    /// Retrieve the navigation links using <see cref="IGetNavigationQuery"/> then return the view
    /// </summary>
    /// <returns>The FooterLinks ViewViewComponentResult</returns>
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var items = await _viewBuilder.GetNavigationLinksAsync();
        return View(items);
    }
}
