using Dfe.PlanTech.Web.Authorisation.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class ValidateApiKeyAttribute : ServiceFilterAttribute
{
    public ValidateApiKeyAttribute() : base(typeof(ApiKeyAuthorisationFilter))
    {
    }
}
