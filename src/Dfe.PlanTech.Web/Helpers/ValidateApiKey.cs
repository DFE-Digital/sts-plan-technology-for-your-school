using Dfe.PlanTech.Web.Authorisation;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Helpers;

public class ValidateApiKey : ServiceFilterAttribute
{
    public ValidateApiKey() : base(typeof(ApiKeyAuthorisationFilter))
    {
    }
}
