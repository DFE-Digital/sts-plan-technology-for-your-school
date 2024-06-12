using System.Text.RegularExpressions;
using Dfe.PlanTech.Domain.SignIns.Models;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.PlanTech.Infrastructure.SignIns;

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
        var config = context.HttpContext.RequestServices.GetRequiredService<IDfeSignInConfiguration>();

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
            var config = context.HttpContext.RequestServices.GetRequiredService<IDfeSignInConfiguration>();

            context.ProtocolMessage.PostLogoutRedirectUri = CreateCallbackUrl(context, config, config.SignoutRedirectUrl);
        }

        return Task.FromResult(0);
    }


    /// <summary>
    /// Gets the origin URL to be used for OpenID Connect redirects.
    /// </summary>
    /// <remarks>
    /// Uses the X-Forwarded-Host header if available, otherwise the FrontDoorUrl field from the <see cref="IDfeSignInConfiguration">config</see>
    /// <param name="context">The context of the redirect request</param>
    /// <param name="config">The configuration for OpenID Connect sign-ins</param>
    public static string GetOriginUrl(RedirectContext context, IDfeSignInConfiguration config)
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
    public static string CreateCallbackUrl(RedirectContext context, IDfeSignInConfiguration config, string callbackPath)
    {
        var originUrl = GetOriginUrl(context, config);

        //Our config uses URls like "/auth/cb" for the redirect callback slug, signout slug, etc.
        //So we should ensure that the URL does not end with a forward slash when returning
        if (originUrl.EndsWith('/') && callbackPath.StartsWith('/'))
        {
            originUrl = originUrl[..^1];
        }
        else if (!originUrl.EndsWith('/') && !callbackPath.StartsWith('/'))
        {
            originUrl += '/';
        }

        if (!SchemeMatchRegex.Match(originUrl).Success)
        {
            originUrl = $"https://{originUrl}";
        }

        return originUrl + callbackPath;
    }
}