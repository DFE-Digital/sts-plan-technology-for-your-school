namespace Dfe.PlanTech.Application.Cookie.Interfaces
{
    public interface ICookieService
    {
        bool SetPreference(string userPreference);

        bool GetCookiePreferenceValue();
    }
}
