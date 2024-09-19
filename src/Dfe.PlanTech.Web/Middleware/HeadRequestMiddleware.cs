namespace Dfe.PlanTech.Web.Middleware;

/// <summary>
/// Middleware to handle head requests automatically
/// </summary>
public class HeadRequestMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await HandleHeadRequests(context);
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
