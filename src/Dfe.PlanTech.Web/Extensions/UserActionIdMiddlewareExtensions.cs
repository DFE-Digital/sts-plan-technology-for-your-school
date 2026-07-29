using Dfe.PlanTech.Web.Middleware;

namespace Dfe.PlanTech.Web.Extensions;

public static class UserActionIdMiddlewareExtensions
{
    public static IApplicationBuilder UseUserActionId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<UserActionIdMiddleware>();
    }
}
