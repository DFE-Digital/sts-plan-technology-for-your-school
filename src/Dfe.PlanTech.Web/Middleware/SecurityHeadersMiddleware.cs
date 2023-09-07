namespace Dfe.PlanTech.Web.Middleware;

public class SecurityHeadersMiddleware
{
  private readonly ILogger<SecurityHeadersMiddleware> _logger;
  private readonly RequestDelegate _next;

  public SecurityHeadersMiddleware(ILogger<SecurityHeadersMiddleware> logger, RequestDelegate next)
  {
    _logger = logger;
    _next = next;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    context.Response.Headers["X-Frame-Options"] = "Deny";
    context.Response.Headers["Content-Security-Policy"] = "frame-ancestors 'none'";

    await _next(context);
  }
}
