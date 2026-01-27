using System.Text;
using Dfe.PlanTech.Application.Configuration;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.Middleware;

/// <summary>
/// Middleware to handle /robots.txt path and generate the file
/// </summary>
#pragma warning disable CS9113 // Parameter is unread.
public class RobotsTxtMiddleware(RequestDelegate next, IOptions<RobotsConfiguration> options)
#pragma warning restore CS9113 // Parameter is unread.
{
    private const string UserAgentKey = "User-agent";
    private const string DisallowKey = "Disallow";
    private const string ContentType = "text/plain";

    public async Task InvokeAsync(HttpContext context)
    {
        await CreateRobotsTxtResponse(context);
    }

    /// <summary>
    /// Generate and return Robots.txt
    /// </summary>
    private async Task CreateRobotsTxtResponse(HttpContext context)
    {
        context.Response.ContentType = ContentType;
        context.Response.Headers.CacheControl = $"max-age={options.Value.CacheMaxAge}";

        var result = GetResponseBody(options.Value);
        await context.Response.Body.WriteAsync(result, context.RequestAborted);
    }

    private static ReadOnlyMemory<byte> GetResponseBody(RobotsConfiguration config) =>
        new(Encoding.UTF8.GetBytes(BuildRobotsTxt(config)));

    private static string BuildRobotsTxt(RobotsConfiguration config)
    {
        var stringBuilder = new StringBuilder();

        AppendUserAgent(config, stringBuilder);
        foreach (var path in config.DisallowedPaths)
        {
            AppendDisallow(stringBuilder, path);
        }

        var result = stringBuilder.ToString();
        return result;
    }

    private static void AppendKeyValue(StringBuilder stringBuilder, string key, string value)
    {
        stringBuilder.Append(key);
        stringBuilder.Append(": ");
        stringBuilder.Append(value);
    }

    private static void AppendUserAgent(RobotsConfiguration config, StringBuilder stringBuilder)
    {
        AppendKeyValue(stringBuilder, UserAgentKey, config.UserAgent);
    }

    private static void AppendDisallow(StringBuilder stringBuilder, string path)
    {
        stringBuilder.AppendLine();
        AppendKeyValue(stringBuilder, DisallowKey, path);
    }
}

public static class RobotsTxtMiddlewareExtensions
{
    public const string RobotsTxtPath = "/robots.txt";

    public static bool IsRobotsPath(HttpContext context) => context.Request.Path == RobotsTxtPath;

    public static IApplicationBuilder UseRobotsTxtMiddleware(this IApplicationBuilder builder) =>
        builder.UseWhen(IsRobotsPath, app => app.UseMiddleware<RobotsTxtMiddleware>());
}
