using Microsoft.AspNetCore.Mvc.Filters;

namespace Dfe.PlanTech.Web.Helpers;

public sealed class LogInvalidModelState : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var svc = context.HttpContext.RequestServices;
        var logger = svc.GetService<ILogger<LogInvalidModelState>>();
        if (!context.ModelState.IsValid)
        {
            var routeName = context.ActionDescriptor.AttributeRouteInfo?.Name;
            logger?.LogError("Not able to validate model state for route: {routeName}", routeName);
        }
    }
}
