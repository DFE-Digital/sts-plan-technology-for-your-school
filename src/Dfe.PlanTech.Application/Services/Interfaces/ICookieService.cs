using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Services.Interfaces;

public interface ICookieService
{
    DfeCookieModel Cookie { get; }

    DfeCookieModel GetCookie();
    void SetCookieAcceptance(bool userAcceptsCookies);
    void SetVisibility(bool visibility);
}
