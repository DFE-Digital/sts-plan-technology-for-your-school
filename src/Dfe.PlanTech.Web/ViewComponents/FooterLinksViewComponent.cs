using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

/// <summary>
/// View component that retrieves and displays links in the layout's footer
/// </summary>
public class FooterLinksViewComponent(IGetNavigationQuery getNavQuery, ILogger<FooterLinksViewComponent> logger) : ViewComponent
{
    private readonly IGetNavigationQuery _getNavQuery = getNavQuery;
    private readonly ILogger<FooterLinksViewComponent> _logger = logger;

    /// <summary>
    /// Retrieve the navigation links using <see cref="IGetNavigationQuery"/> then return the view
    /// </summary>
    /// <returns>The FooterLinks ViewViewComponentResult</returns>
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var items = await GetLinks();

        return View(items);
    }

    /// <summary>
    /// Gets links from Contentful using <see cref="IGetNavigationQuery"/> with basic error handling 
    /// </summary>
    /// <returns></returns>
    private async Task<IEnumerable<INavigationLink>> GetLinks()
    {
        try
        {
            return await _getNavQuery.GetNavigationLinks();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error retrieving navigation links for footer");

            return await Task.FromResult(Array.Empty<NavigationLink>().AsEnumerable());
        }
    }
}
