namespace Dfe.PlanTech.Domain.Cookie.Interfaces;

public interface ICookieService
{
    public DfeCookie GetCookie();

    public void SetPreference(bool userPreference);

    public void SetVisibility(bool visibility);

    public void RejectCookies();
}
