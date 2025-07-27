using Dfe.PlanTech.Application.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dfe.PlanTech.Web.Authorisation.Filters;

public class ApiKeyAuthorisationFilter(ApiAuthenticationConfiguration config, ILogger<ApiKeyAuthorisationFilter> logger) : IAuthorizationFilter
{
    public const string AuthHeaderKey = "Authorization";
    public const string AuthValuePrefix = "Bearer ";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!HaveValidConfiguration() || !AuthorisationHeaderValid(context))
        {
            context.Result = new UnauthorizedResult();
        }
    }

    private bool HaveValidConfiguration()
    {
        if (config.HaveApiKey)
        {
            return true;
        }

        logger.LogError("API key {KeyName} is missing", nameof(config.KeyValue));
        return false;
    }

    private bool AuthorisationHeaderValid(AuthorizationFilterContext context)
    {
        if (!TryGetProvidedApiKey(context, out string? providedApiKey) || string.IsNullOrEmpty(providedApiKey))
        {
            logger.LogInformation("Request to authorised route denied due to missing authentication header");
            return false;
        }

        return config.ApiKeyMatches(providedApiKey);
    }

    private static bool TryGetProvidedApiKey(AuthorizationFilterContext context, out string? apiKey)
    {
        var authorisationHeaderValue = context.HttpContext.Request.Headers[AuthHeaderKey].ToString();

        if (string.IsNullOrEmpty(authorisationHeaderValue) || !authorisationHeaderValue.StartsWith(AuthValuePrefix))
        {
            apiKey = null;
            return false;
        }

        apiKey = authorisationHeaderValue[AuthValuePrefix.Length..];
        return true;
    }
}
