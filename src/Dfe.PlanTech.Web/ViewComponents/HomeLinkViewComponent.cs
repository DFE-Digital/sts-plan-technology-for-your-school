using Dfe.PlanTech.Web.Models;

namespace Dfe.PlanTech.Web.ViewComponents;

using Microsoft.AspNetCore.Mvc;

public class HomeLinkViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(AccessibilityViewModel model)
    {
        if (model.UserIsAuthenticated)
        {
            return View("AuthenticatedLink");
        }
        else
        {
            return View("UnauthenticatedLink");
        }
    }
}