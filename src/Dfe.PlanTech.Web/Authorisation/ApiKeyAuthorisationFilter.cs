using Dfe.PlanTech.Domain.Persistence.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;

namespace Dfe.PlanTech.Web.Authorisation;

public class ApiKeyAuthorisationFilter([FromServices] CacheRefreshConfiguration cacheRefreshConfiguration) : IAuthorizationFilter
{
    private const string ApiKeyName = "X-WEBSITE-CACHE-CLEAR-API-KEY";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyName, out var providedApiKey))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var validApiKey = cacheRefreshConfiguration.ApiKey;
        if (validApiKey is null || !validApiKey.Equals(providedApiKey))
        {
            context.Result = new UnauthorizedResult();
        }
    }
}
