using Dfe.PlanTech.Application.Cookie.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Dfe.PlanTech.Application.Cookie.Service
{
    public class CookieService : ICookieService
    {
        private HttpContext _context;

        public CookieService(HttpContext context)
        {
            _context = context;
        }

        public bool GetCookiePreferenceValue()
        {
            bool.TryParse(_context.Request.Cookies["cookies_preferences_set"], out bool cookieValue);
            return cookieValue;
        }

        public bool SetPreference(string userPreference)
        {
            if (userPreference == "yes")
            {
                CreateCookie("cookies_preferences_set", "true");
                return true;
            }
            else
            {
                CreateCookie("cookies_preferences_set", "false");
                return false;
            }
        }

        private void CreateCookie(string key, string value)
        {
            CookieOptions cookieOptions = new CookieOptions();
            cookieOptions.Secure = true;
            cookieOptions.HttpOnly = true;
            cookieOptions.Expires = new DateTimeOffset(DateTime.Now.AddYears(1));
            _context.Response.Cookies.Append(key, value, cookieOptions);
        }
    }
}