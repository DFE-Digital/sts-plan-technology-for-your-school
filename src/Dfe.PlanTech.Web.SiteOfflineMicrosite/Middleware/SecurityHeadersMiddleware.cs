namespace Dfe.PlanTech.Web.SiteOfflineMicrosite.Middleware;

/// <summary>
/// Middleware to add security headers and remove information disclosure headers.
/// Based on OWASP Secure Headers Project recommendations.
/// References:
/// - https://owasp.org/www-project-secure-headers/
/// - Headers to add: https://owasp.org/www-project-secure-headers/ci/headers_add.json
/// - Headers to remove: https://owasp.org/www-project-secure-headers/ci/headers_remove.json
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Headers to remove to prevent technology fingerprinting and information disclosure.
    /// Based on OWASP recommendations at https://owasp.org/www-project-secure-headers/ci/headers_remove.json
    /// </summary>
    private static readonly string[] HeadersToRemove =
    [
        "Server",
        "X-Powered-By",
        "X-AspNet-Version",
        "X-AspNetMvc-Version",
        "Powered-By",
        "Product",
        "X-Atmosphere-error",
        "X-Atmosphere-first-request",
        "X-Atmosphere-tracking-id",
        "X-B3-ParentSpanId",
        "X-B3-Sampled",
        "X-B3-SpanId",
        "X-B3-TraceId",
        "X-Content-Encoded-By",
        "X-Envoy-Attempt-Count",
        "X-Envoy-External-Address",
        "X-Envoy-Internal",
        "X-Envoy-Original-Dst-Host",
        "X-Envoy-Upstream-Service-Time",
        "X-Framework",
        "X-Generated-By",
        "X-Generator",
        "X-Nextjs-Cache",
        "X-Nextjs-Matched-Path",
        "X-Nextjs-Page",
        "X-Nextjs-Redirect",
        "X-Old-Content-Length",
        "X-Php-Version",
        "X-Powered-CMS",
        "X-Redirect-By",
        "X-Server-Powered-By",
        "X-SourceFiles",
        "X-SourceMap",
        "SourceMap"
    ];

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;

            foreach (var header in HeadersToRemove)
            {
                headers.Remove(header);
            }

            if (!headers.ContainsKey("X-Content-Type-Options"))
            {
                headers.Append("X-Content-Type-Options", "nosniff");
            }

            if (!headers.ContainsKey("X-Frame-Options"))
            {
                headers.Append("X-Frame-Options", "deny");
            }

            if (!headers.ContainsKey("X-DNS-Prefetch-Control"))
            {
                headers.Append("X-DNS-Prefetch-Control", "off");
            }

            if (!headers.ContainsKey("X-Permitted-Cross-Domain-Policies"))
            {
                headers.Append("X-Permitted-Cross-Domain-Policies", "none");
            }

            if (!headers.ContainsKey("Content-Security-Policy"))
            {
                headers.Append("Content-Security-Policy",
                    "default-src 'self'; " +
                    "style-src 'self' 'unsafe-inline'; " +
                    "script-src 'self' 'unsafe-inline'; " +
                    "img-src 'self' data:; " +
                    "font-src 'self' data:; " +
                    "form-action 'self'; " +
                    "base-uri 'self'; " +
                    "object-src 'none'; " +
                    "frame-ancestors 'none'; " +
                    "upgrade-insecure-requests");
            }

            if (!headers.ContainsKey("Cross-Origin-Embedder-Policy"))
            {
                headers.Append("Cross-Origin-Embedder-Policy", "require-corp");
            }

            if (!headers.ContainsKey("Cross-Origin-Opener-Policy"))
            {
                headers.Append("Cross-Origin-Opener-Policy", "same-origin");
            }

            if (!headers.ContainsKey("Cross-Origin-Resource-Policy"))
            {
                headers.Append("Cross-Origin-Resource-Policy", "same-origin");
            }

            if (!headers.ContainsKey("Referrer-Policy"))
            {
                headers.Append("Referrer-Policy", "no-referrer");
            }

            if (!headers.ContainsKey("Permissions-Policy"))
            {
                headers.Append("Permissions-Policy",
                    "accelerometer=(), autoplay=(), camera=(), cross-origin-isolated=(), display-capture=(), " +
                    "encrypted-media=(), fullscreen=(), geolocation=(), gyroscope=(), keyboard-map=(), " +
                    "magnetometer=(), microphone=(), midi=(), payment=(), picture-in-picture=(), " +
                    "publickey-credentials-get=(), screen-wake-lock=(), sync-xhr=(self), usb=(), " +
                    "web-share=(), xr-spatial-tracking=(), clipboard-read=(), clipboard-write=(), " +
                    "gamepad=(), hid=(), idle-detection=(), interest-cohort=(), serial=(), unload=()");
            }

            if (!headers.ContainsKey("Strict-Transport-Security"))
            {
                headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            }

            if (!headers.ContainsKey("Cache-Control") && !context.Request.Path.StartsWithSegments("/health"))
            {
                headers.Append("Cache-Control", "no-store, max-age=0");
            }

            return Task.CompletedTask;
        });

        await _next(context);
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}

