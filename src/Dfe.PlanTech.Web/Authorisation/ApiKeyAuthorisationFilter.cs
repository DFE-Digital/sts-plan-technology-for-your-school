using Dfe.PlanTech.Domain.Persistence.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;

namespace Dfe.PlanTech.Web.Authorisation;

public class ApiKeyAuthorisationFilter(ApiAuthenticationConfiguration apiAuthenticationConfiguration, ILogger<ApiKeyAuthorisationFilter> logger) : IAuthorizationFilter
{
    public const string AuthHeaderKey = "Authorization";
    public const string AuthValuePrefix = "Bearer ";
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!GetProvidedApiKey(context, out var providedApiKey))
        {
            logger.LogInformation("Request to authorised route denied due to missing authentication header");
            context.Result = new UnauthorizedResult();
        }

        var validApiKey = apiAuthenticationConfiguration.KeyValue;

        if (validApiKey.IsNullOrEmpty())
        {
            logger.LogError("ApiKey config value missing");
            context.Result = new UnauthorizedResult();
        }

        if (!validApiKey!.Equals(providedApiKey))
        {
            context.Result = new UnauthorizedResult();
        }
    }

    private static bool GetProvidedApiKey(AuthorizationFilterContext context, out string? apiKey)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(AuthHeaderKey, out var providedApiKey) ||
            string.IsNullOrEmpty(providedApiKey))
        {
            apiKey = null;
            return false;
        }

        var value = providedApiKey.ToString();

        if (!value.StartsWith(AuthValuePrefix))
        {
            apiKey = null;
            return false;
        }

        apiKey = value[AuthValuePrefix.Length..];
        return true;
    }
}
