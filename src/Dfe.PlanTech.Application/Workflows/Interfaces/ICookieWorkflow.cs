using Microsoft.AspNetCore.Http;

namespace Dfe.PlanTech.Application.Workflows.Interfaces
{
    public interface ICookieWorkflow
    {
        void RemoveNonEssentialCookies(HttpContext context);
    }
}