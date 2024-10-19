using System.Globalization;
using Dfe.PlanTech.Domain.Persistence.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;

namespace Dfe.PlanTech.Web.Authorisation;

public class SigningSecretAuthorisationFilter(SigningSecretConfiguration signingSecretConfiguration, ILogger<ApiKeyAuthorisationFilter> logger) : IAuthorizationFilter
{
    private const string HeaderSignature = "x-contentful-signature";
    private const string HeaderTimestap = "x-contentful-timestap";
    private const int RequestTimeToLiveMinutes = 5;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!GetRequestSignature(context, out var signature))
        {
            logger.LogInformation("Request to CMS route denied due to missing or expired signing secret");
            context.Result = new UnauthorizedResult();
        }

        var signingSecret = signingSecretConfiguration.SigningSecret;

        if (signingSecret.IsNullOrEmpty())
        {
            logger.LogError("Signing secret config value missing");
            context.Result = new UnauthorizedResult();
        }

        if (!signingSecret!.Equals(signature))
        {
            context.Result = new UnauthorizedResult();
        }
    }

    private static bool GetRequestSignature(AuthorizationFilterContext context, out string? signature)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderSignature, out var requestSignature) ||
            !context.HttpContext.Request.Headers.TryGetValue(HeaderTimestap, out var requestTimestamp) ||
            string.IsNullOrEmpty(requestSignature) ||
            string.IsNullOrEmpty(requestTimestamp))
        {
            signature = null;
            return false;
        }

        var value = requestSignature.ToString();
        var timestamp = DateTime.Parse(requestTimestamp.ToString(), CultureInfo.InvariantCulture);

        if (timestamp.AddMinutes(RequestTimeToLiveMinutes) <= DateTime.UtcNow)
        {
            signature = null;
            return false;
        }

        signature = value;
        return true;
    }
}
