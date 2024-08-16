using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Web.Controllers;

public class BaseController<TConcreteController> : Controller
{
    protected readonly ILogger<BaseController<TConcreteController>> logger;

    public BaseController(ILogger<BaseController<TConcreteController>> logger)
    {
        this.logger = logger;
    }
}
