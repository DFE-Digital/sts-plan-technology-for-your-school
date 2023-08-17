using Dfe.PlanTech.Domain.Content.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

/// <summary>
/// View component that retrieves and displays links in the layout's footer
/// </summary>
public class FooterLinks : ViewComponent
{
  private readonly IGetNavigationQuery _getNavQuery;

  public FooterLinks(IGetNavigationQuery getNavQuery) => _getNavQuery = getNavQuery;

  /// <summary>
  /// Retrieve the navigation links using <see cref="IGetNavigationQuery"/> then return the view
  /// </summary>
  /// <returns>The FooterLinks ViewViewComponentResult</returns>
  public async Task<IViewComponentResult> InvokeAsync()
  {
    var items = await _getNavQuery.GetNavigationLinks();

    return View(items);
  }
}
