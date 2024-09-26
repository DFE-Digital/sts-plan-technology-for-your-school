using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

public class BaseController<TConcreteController>(ILogger<BaseController<TConcreteController>> logger) : Controller
{
    protected readonly ILogger<BaseController<TConcreteController>> Logger = logger;
}
