using Dfe.PlanTech.Web.Authorisation;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Helpers;

[AttributeUsage(AttributeTargets.Method)]
public class ValidateSigningSecretAttribute : ServiceFilterAttribute
{
    public ValidateSigningSecretAttribute() : base(typeof(SigningSecretAuthorisationFilter))
    {
    }
}
