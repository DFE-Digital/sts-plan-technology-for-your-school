using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

public class BaseController<TConcreteController> : Controller
{
    protected readonly ILogger<TConcreteController> logger;

    public BaseController(ILogger<TConcreteController> logger)
    {
        this.logger = logger;
    }
}
