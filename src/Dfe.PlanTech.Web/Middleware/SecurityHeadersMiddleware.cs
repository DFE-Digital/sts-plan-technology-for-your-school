namespace Dfe.PlanTech.Web.Middleware;

/// <summary>
/// Middleware that sets various response headers for security reasons.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)

    {
        AddFramejackingPreventHeaders(context);
        AddContentSecurityPolicy(context);
        await _next(context);
    }

    /// <summary>
    /// Adds headers that prevent framejacking vulnerability
    /// </summary>
    /// <param name="context"></param>
    private static void AddFramejackingPreventHeaders(HttpContext context)
    {
        context.Response.Headers.XFrameOptions = "Deny";
    }

    private static string GenerateNonce(HttpContext context)
    {
        var nonce = Guid.NewGuid().ToString("N");
        context.Items["nonce"] = nonce;
        return nonce;
    }

    /// <summary>
    /// Returns space separated hashes of any scripts that cannot be inlined, but are allowed
    /// </summary>
    /// <param name="context"></param>
    private static string GetAllowedScriptHashes(HttpContext context)
    {
        var allowedScriptHashes = new List<string>
        {
            // js enabled script from GovUK page template helper
            "sha256-wmo5EWLjw+Yuj9jZzGNNeSsUOBQmBvE1pvSPVNQzJ34=",
        };
        return string.Join(" ", allowedScriptHashes.Select(hash => $"'{hash}'"));
    }

    /// <summary>
    /// Adds csp directives that prevent unsafe-inline execution and frame-jacking and a nonce for whitelisting scripts
    /// </summary>
    /// <param name="context"></param>
    private static void AddContentSecurityPolicy(HttpContext context)
    {
        var nonce = GenerateNonce(context);
        var whitelist = GetAllowedScriptHashes(context);
        var cspDirectives = new List<string>
        {
            "frame-ancestors 'none'",
            "default-src 'self' https://www.clarity.ms https://c.bing.com",
            $"script-src 'nonce-{nonce}' {whitelist}",
            "img-src 'self' www.googletagmanager.com",
            "connect-src www.googletagmanager.com"
        };
        var csp = string.Join("; ", cspDirectives);
        context.Response.Headers.ContentSecurityPolicy = csp;
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