using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

public class BaseController<TConcreteController>(
    ILoggerFactory loggerFactory
) : Controller
{
    protected readonly ILogger<TConcreteController> Logger = loggerFactory.CreateLogger<TConcreteController>();
}
