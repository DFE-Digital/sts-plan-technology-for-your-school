using Dfe.PlanTech.Web.Authorisation;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Helpers;

public class ValidateApiKeyAttribute : ServiceFilterAttribute
{
    public ValidateApiKeyAttribute() : base(typeof(ApiKeyAuthorisationFilter))
    {
    }
}
