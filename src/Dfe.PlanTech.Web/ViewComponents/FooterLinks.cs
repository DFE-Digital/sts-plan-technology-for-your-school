using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

/// <summary>
/// View component that retrieves and displays links in the layout's footer
/// </summary>
public class FooterLinks : ViewComponent
{
  private readonly IGetNavigationQuery _getNavQuery;
  private readonly ILogger<FooterLinks> _logger;

  public FooterLinks(IGetNavigationQuery getNavQuery, ILogger<FooterLinks> logger)
  {
    _getNavQuery = getNavQuery;
    _logger = logger;
  }

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
  private Task<IEnumerable<NavigationLink>> GetLinks()
  {
    try
    {
      return _getNavQuery.GetNavigationLinks();
    }
    catch (Exception ex)
    {
      _logger.LogCritical(ex, "Error retrieving navigation links for footer");

      return Task.FromResult(Array.Empty<NavigationLink>().AsEnumerable());
    }
  }
}
