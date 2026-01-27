using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

public class BaseController<TConcreteController>(ILogger<TConcreteController> logger) : Controller
{
    protected readonly ILogger<TConcreteController> Logger = logger;
}
