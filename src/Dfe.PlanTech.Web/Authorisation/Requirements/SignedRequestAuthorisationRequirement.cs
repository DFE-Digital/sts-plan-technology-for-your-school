using Microsoft.AspNetCore.Authorization;

namespace Dfe.PlanTech.Web.Authorisation.Requirements;

public class SignedRequestAuthorisationRequirement : IAuthorizationRequirement
{
    public SignedRequestAuthorisationRequirement()
    {
    }
}
