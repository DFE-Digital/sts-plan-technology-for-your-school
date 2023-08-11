using Dfe.PlanTech.Domain.Cookie;

namespace Dfe.PlanTech.Application.Cookie.Interfaces
{
    public interface ICookieService
    {
        DfeCookie GetCookie();

        void SetPreference(bool userPreference);

        void SetVisibility(bool visibility);

        void RejectCookies();
    }
}
