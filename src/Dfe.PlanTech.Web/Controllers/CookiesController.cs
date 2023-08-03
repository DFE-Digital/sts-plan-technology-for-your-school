namespace Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


[Authorize]
[Route("/cookies")]
public class CookiesController : BaseController<CookiesController>
{

    public CookiesController(ILogger<CookiesController> logger) : base(logger) { }

    public Task<IActionResult> GetCookiesPage()
    {
        return Task.FromResult<IActionResult>(View("Cookies"));
    }
    
}