using Dfe.PlanTech.Application.Providers.Interfaces;
using Microsoft.AspNetCore.WebUtilities;

namespace Dfe.PlanTech.Web.Middleware;

/// <summary>
/// Middleware to handle head requests automatically
/// </summary>
public class RequestMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (await TryHandleContentfulRedirect(context))
        {
            return;
        }

        await HandleHeadRequests(context);
    }

    /// <summary>
    /// Handle redirects stored in Contentful
    /// </summary>
    private static async Task<bool> TryHandleContentfulRedirect(HttpContext context)
    {
        using var scope = context.RequestServices.CreateAsyncScope();
        var redirectProvider = scope.ServiceProvider.GetRequiredService<IRedirectProvider>();

        var path = context.Request.Path.Value?.TrimStart('/');
        if (string.IsNullOrWhiteSpace(path) || redirectProvider.IsStaticPath(path))
        {
            return false;
        }

        var newPath = await redirectProvider.TryGetRedirect(path);
        if (newPath is null)
        {
            return false;
        }

        context.Response.StatusCode = StatusCodes.Status301MovedPermanently;
        context.Response.Headers.Location = QueryHelpers.AddQueryString(
            newPath,
            context.Request.Query
        );
        return true;
    }

    /// <summary>
    /// Strip out the body for any Head Requests
    /// </summary>
    private async Task HandleHeadRequests(HttpContext context)
    {
        if (context.Request.Method == HttpMethod.Head.Method)
        {
            context.Request.Method = HttpMethod.Get.Method;
            await next(context);
            context.Response.Body = Stream.Null;
        }
        else
        {
            await next(context);
        }
    }
}
