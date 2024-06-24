namespace Dfe.PlanTech.Domain.Cookie.Interfaces;

public interface ICookieService
{
    public DfeCookie Cookie { get; }

    public void SetCookieAcceptance(bool userAcceptsCookies);

    public void SetVisibility(bool visibility);
}
