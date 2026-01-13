using Microsoft.AspNetCore.Mvc.Filters;

namespace Dfe.PlanTech.Web.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public sealed class LogInvalidModelStateAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var svc = context.HttpContext.RequestServices;
        var logger = svc.GetService<ILogger<LogInvalidModelStateAttribute>>();

        ArgumentNullException.ThrowIfNull(logger);

        if (!context.ModelState.IsValid)
        {
            var displayName = context.ActionDescriptor.DisplayName;
            logger.LogError("Not able to validate model state for controller method: {DisplayName}", displayName);
        }
    }
}
