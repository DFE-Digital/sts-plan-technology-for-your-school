using Dfe.PlanTech.Web.ViewBuilders;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

/// <summary>
/// View component that retrieves and displays links in the layout's footer
/// </summary>
public class FooterLinksViewComponent(
    ILogger<FooterLinksViewComponent> logger,
    FooterLinksViewComponentViewBuilder viewBuilder
) : ViewComponent
{
    private readonly ILogger<FooterLinksViewComponent> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly FooterLinksViewComponentViewBuilder _viewBuilder = viewBuilder ?? throw new ArgumentNullException(nameof(viewBuilder));

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
