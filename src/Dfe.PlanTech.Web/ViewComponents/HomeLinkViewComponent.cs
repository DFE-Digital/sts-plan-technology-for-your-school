namespace Dfe.PlanTech.Web.ViewComponents;

using Microsoft.AspNetCore.Mvc;

public class HomeLinkViewComponent : ViewComponent
{  
    public bool IsAuthenticated => User.Identity?.IsAuthenticated == true;
    public string Href => IsAuthenticated ? "/self-assessment" : "/";

    public IViewComponentResult Invoke() => View("Default", Href);
}