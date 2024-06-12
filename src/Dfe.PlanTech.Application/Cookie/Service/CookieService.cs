using Dfe.PlanTech.Domain.Cookie;
using Dfe.PlanTech.Domain.Cookie.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Dfe.PlanTech.Application.Cookie.Service;

public class CookieService : ICookieService
{
    public const string Cookie_Key = "user_cookies_preferences";

    private readonly IHttpContextAccessor _context;
    private DfeCookie? _dfeCookie;

    public DfeCookie Cookie => _dfeCookie ?? GetCookie();

    public CookieService(IHttpContextAccessor context)
    {
        _context = context;
    }

    public void SetVisibility(bool visibility)
    {
        CreateCookie(Cookie_Key, visibility: visibility);
    }

    public void SetCookieAcceptance(bool userAcceptsCookies)
    {
        CreateCookie(Cookie_Key, userAcceptsCookies: userAcceptsCookies);
    }

    public DfeCookie GetCookie()
    {
        var cookie = _context.HttpContext.Request.Cookies[Cookie_Key];
        if (cookie is null)
        {
            return new DfeCookie();
        }

        var dfeCookie = JsonSerializer.Deserialize<DfeCookie>(cookie);
        return dfeCookie;
    }

    private void DeleteCookie()
    {
        _context.HttpContext.Response.Cookies.Delete(Cookie_Key);
    }

    private void CreateCookie(string key, bool? userAcceptsCookies = null, bool? visibility = null)
    {
        CookieOptions cookieOptions = new CookieOptions
        {
            Secure = true,
            HttpOnly = true,
            Expires = new DateTimeOffset(DateTime.Now.AddYears(1))
        };

        DeleteCookie();

        var cookie = Cookie with
        {
            UserAcceptsCookies = userAcceptsCookies ?? Cookie.UserAcceptsCookies,
            IsVisible = visibility ?? Cookie.IsVisible
        };

        var serializedCookie = JsonSerializer.Serialize(cookie);
        _context.HttpContext.Response.Cookies.Append(key, serializedCookie, cookieOptions);
        _dfeCookie = cookie;
    }
}