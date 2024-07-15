using Dfe.PlanTech.Domain.Persistence.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;

namespace Dfe.PlanTech.Web.Authorisation;

public class ApiKeyAuthorisationFilter([FromServices] CacheRefreshConfiguration cacheRefreshConfiguration) : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (cacheRefreshConfiguration.ApiKeyName.IsNullOrEmpty() || !context.HttpContext.Request.Headers.TryGetValue(cacheRefreshConfiguration.ApiKeyName!, out var providedApiKey))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var validApiKey = cacheRefreshConfiguration.ApiKeyValue;
        if (validApiKey.IsNullOrEmpty() || !validApiKey!.Equals(providedApiKey))
        {
            context.Result = new UnauthorizedResult();
        }
    }
}
