using Microsoft.AspNetCore.Authorization;

namespace Dfe.PlanTech.Web.Authorisation;

public class SignedRequestAuthorisationRequirement : IAuthorizationRequirement
{
    public SignedRequestAuthorisationRequirement()
    {
    }
}
