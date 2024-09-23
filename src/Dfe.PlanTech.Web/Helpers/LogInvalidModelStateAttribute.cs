using Microsoft.AspNetCore.Mvc.Filters;

namespace Dfe.PlanTech.Web.Helpers;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public sealed class LogInvalidModelStateAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var svc = context.HttpContext.RequestServices;
        var logger = svc.GetService<ILogger<LogInvalidModelStateAttribute>>();
        if (!context.ModelState.IsValid)
        {
            var displayName = context.ActionDescriptor.DisplayName;
            logger?.LogError("Not able to validate model state for controller method: {displayName}", displayName);
        }
    }
}
