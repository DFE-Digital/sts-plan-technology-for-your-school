namespace Dfe.PlanTech.Web.Middleware;

/// <summary>
/// Middleware that sets various response headers for security reasons.
/// </summary>
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
    AddFramejackingPreventHeaders(context);

    await _next(context);
  }

  /// <summary>
  /// Adds headers that prevent framejacking vulnerability
  /// </summary>
  /// <param name="context"></param>
  private static void AddFramejackingPreventHeaders(HttpContext context)
  {
    context.Response.Headers["X-Frame-Options"] = "Deny";
    context.Response.Headers["Content-Security-Policy"] = "frame-ancestors 'none'";
  }
}

public static class SecurityHeadersMiddlewareExtensions
{
  /// <summary>
  /// Extension method for adding the above middleware in a cleaner way.
  /// </summary>
  /// <param name="app"></param>
  /// <returns></returns>
  public static IApplicationBuilder UseSecurityHeaders(
      this IApplicationBuilder app) => app.UseMiddleware<SecurityHeadersMiddleware>();
}