using Dfe.PlanTech.Web.Context.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Dfe.PlanTech.Web.Middleware;

public class UserActionIdMiddleware(RequestDelegate next)
{
    public const string HttpContextItemKey = "CorrelationId";
    public const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);

        context.Items[HttpContextItemKey] = correlationId;
        context.Response.Headers[HeaderName] = correlationId.ToString();
        await next(context);
    }

    private static Guid GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var headerValues)
            && Guid.TryParse(headerValues.FirstOrDefault(), out var headerGuid))
        {
            return headerGuid;
        }

        return Guid.NewGuid();
    }
}
