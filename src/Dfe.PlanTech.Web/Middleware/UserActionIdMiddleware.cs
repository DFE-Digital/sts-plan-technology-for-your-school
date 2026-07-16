using Dfe.PlanTech.Core.Constants;

namespace Dfe.PlanTech.Web.Middleware;

public class UserActionIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var userActionId = GetOrCreateUserActionId(context);

        context.Items[UserActionIdConstants.HttpContextItemKey] = userActionId;
        context.Response.Headers[UserActionIdConstants.HeaderName] = userActionId.ToString();

        await next(context);
    }

    private static Guid GetOrCreateUserActionId(HttpContext context)
    {
        if (
            context.Request.Headers.TryGetValue(UserActionIdConstants.HeaderName, out var headerValues)
            && Guid.TryParse(headerValues.FirstOrDefault(), out var headerGuid)
        )
        {
            return headerGuid;
        }

        return Guid.NewGuid();
    }
}
