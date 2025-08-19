using System.Text.RegularExpressions;
using Dfe.PlanTech.Application.Configuration;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.PlanTech.Infrastructure.SignIn;

public static partial class DfeOpenIdConnectEvents
{
    [GeneratedRegex(SchemeMatchRegexPattern)]
    private static partial Regex SchemeMatchRegexAttribute();

    public const string SchemeMatchRegexPattern = @"^(https?:\/\/)";
    public readonly static Regex SchemeMatchRegex = SchemeMatchRegexAttribute();

    private const string ForwardHostHeader = "X-Forwarded-Host";

    /// <summary>
    /// Re-writes the login callback URI
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static Task OnRedirectToIdentityProvider(RedirectContext context)
    {
        var config = context.HttpContext.RequestServices.GetRequiredService<DfeSignInConfiguration>();

        context.ProtocolMessage.RedirectUri = CreateCallbackUrl(context, config, config.CallbackUrl);

        return Task.FromResult(0);
    }

    /// <summary>
    /// Re-writes the signout redirect URI
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static Task OnRedirectToIdentityProviderForSignOut(RedirectContext context)
    {
        if (context.ProtocolMessage != null)
        {
            var config = context.HttpContext.RequestServices.GetRequiredService<DfeSignInConfiguration>();

            context.ProtocolMessage.PostLogoutRedirectUri = CreateCallbackUrl(context, config, config.SignoutRedirectUrl);
        }

        return Task.FromResult(0);
    }

    /// <summary>
    /// Gets the origin URL to be used for OpenID Connect redirects.
    /// </summary>
    /// <remarks>
    /// Uses the X-Forwarded-Host header if available, otherwise the FrontDoorUrl field from the <see cref="DfeSignInConfiguration">config</see>
    /// <param name="context">The context of the redirect request</param>
    /// <param name="config">The configuration for OpenID Connect sign-ins</param>
    /// </remarks>
    public static string GetOriginUrl(RedirectContext context, DfeSignInConfiguration config)
    {
        var forwardHostHeader = context.HttpContext.Request.Headers
                                                            .Where(header => string.Equals(ForwardHostHeader, header.Key, StringComparison.InvariantCultureIgnoreCase))
                                                            .Select(header => header.Value.FirstOrDefault())
                                                            .FirstOrDefault();

        return forwardHostHeader ?? config.FrontDoorUrl;
    }

    /// <summary>
    /// Creates the callback URL by combining the origin URL and the callback path.
    /// </summary>
    /// <param name="context">The context of the redirect request.</param>
    /// <param name="config">The configuration for OpenID Connect sign-ins.</param>
    /// <param name="callbackPath">The path of the callback URL.</param>
    public static string CreateCallbackUrl(RedirectContext context, DfeSignInConfiguration config, string callbackPath)
    {
        var originUrl = GetOriginUrl(context, config).EnsureScheme();

        var baseUri = new Uri(originUrl);
        var callbackUri = new Uri(baseUri, callbackPath);

        return callbackUri.ToString();
    }

    /// <summary>
    /// Ensures the URL has a URL scheme, if not prefixes the string with "https://"
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private static string EnsureScheme(this string url) => SchemeMatchRegex.Match(url).Success ? url : $"https://{url}";
}
