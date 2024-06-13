using Microsoft.AspNetCore.Http;

namespace Dfe.PlanTech.Application.Cookie.Service;

public interface ICookiesCleaner
{
    public void RemoveNonEssentialCookies(HttpContext context);
}