using Dfe.PlanTech.Domain.Content.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

public class FooterLinks : ViewComponent
{
  private readonly IGetNavigationQuery _getNavQuery;

  public FooterLinks(IGetNavigationQuery getNavQuery) => _getNavQuery = getNavQuery;

  public async Task<IViewComponentResult> InvokeAsync()
  {
    var items = await _getNavQuery.GetNavigationLinks();

    return View(items);
  }
}
