using Dfe.PlanTech.Application.Content.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ViewComponentSample.ViewComponents;

public class FooterLinks : ViewComponent
{
  private GetNavigationQuery _getNavQuery;

  public FooterLinks(GetNavigationQuery getNavQuery) => _getNavQuery = getNavQuery;

  public async Task<IViewComponentResult> InvokeAsync()
  {
    var items = await _getNavQuery.GetNavigationLinks();

    return View(items);
  }
}
