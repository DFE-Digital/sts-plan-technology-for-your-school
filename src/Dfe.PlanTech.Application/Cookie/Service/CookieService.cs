using Dfe.PlanTech.Domain.Cookie;
using Dfe.PlanTech.Domain.Cookie.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Dfe.PlanTech.Application.Cookie.Service
{
    public class CookieService : ICookieService
    {
        private readonly IHttpContextAccessor _context;
        private const string Cookie_Key = "cookies_preferences_set";

        public CookieService(IHttpContextAccessor context)
        {
            _context = context;
        }

        public void SetVisibility(bool visibility)
        {
            var currentCookie = GetCookie();
            DeleteCookie();
            CreateCookie(Cookie_Key, currentCookie.HasApproved, visibility, currentCookie.IsRejected);
        }

        public void RejectCookies()
        {
            DeleteCookie();
            CreateCookie(Cookie_Key, false, true, true);
        }

        public void SetPreference(bool userPreference)
        {
            CreateCookie(Cookie_Key, userPreference);
        }

        public DfeCookie GetCookie()
        {
            var cookie = _context.HttpContext.Request.Cookies[Cookie_Key];
            if (cookie is null)
            {
                return new DfeCookie();
            }
            else
            {
                var dfeCookie = JsonSerializer.Deserialize<DfeCookie>(cookie);
                return dfeCookie is null ? new DfeCookie() : dfeCookie;
            }
        }

        private void DeleteCookie()
        {
            _context.HttpContext.Response.Cookies.Delete(Cookie_Key);
        }

        private void CreateCookie(string key, bool value, bool visibility = true, bool rejected = false)
        {
            CookieOptions cookieOptions = new CookieOptions();
            cookieOptions.Secure = true;
            cookieOptions.HttpOnly = true;
            cookieOptions.Expires = new DateTimeOffset(DateTime.Now.AddYears(1));

            var cookie = new DfeCookie { IsVisible = visibility, HasApproved = value, IsRejected = rejected };
            var serializedCookie = JsonSerializer.Serialize(cookie);
            _context.HttpContext.Response.Cookies.Append(key, serializedCookie, cookieOptions);
        }
    }
}