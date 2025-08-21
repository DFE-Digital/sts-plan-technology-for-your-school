using Microsoft.AspNetCore.Mvc.Filters;

namespace Dfe.PlanTech.Web.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public sealed class LogInvalidModelStateAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var svc = context.HttpContext.RequestServices;
        var loggerFactory = svc.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger<LogInvalidModelStateAttribute>() ?? throw new ArgumentNullException(nameof(loggerFactory));

        if (!context.ModelState.IsValid)
        {
            var displayName = context.ActionDescriptor.DisplayName;
            logger.LogError("Not able to validate model state for controller method: {displayName}", displayName);
        }
    }
}
