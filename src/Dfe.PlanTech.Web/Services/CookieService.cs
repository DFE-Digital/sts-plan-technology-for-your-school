using System.Text.Json;
using Dfe.PlanTech.Domain.Cookie;
using Dfe.PlanTech.Domain.Cookie.Interfaces;
using Dfe.PlanTech.Web.Workflows;

namespace Dfe.PlanTech.Web.Services;

public class CookieService(IHttpContextAccessor context, CookieWorkflow cookiesCleaner) : ICookieService
{
    public const string Cookie_Key = "user_cookies_preferences";

    private DfeCookie? _dfeCookie;
    public DfeCookie Cookie => _dfeCookie ?? GetCookie();

    public void SetVisibility(bool visibility)
    {
        CreateCookie(Cookie_Key, visibility: visibility);
    }

    public void SetCookieAcceptance(bool userAcceptsCookies)
    {
        if (context.HttpContext is null)
        {
            throw new InvalidOperationException($"Cannot set cookie acceptance as {nameof(context.HttpContext)} is null");
        }

        CreateCookie(Cookie_Key, userAcceptsCookies: userAcceptsCookies);

        if (!userAcceptsCookies)
        {
            cookiesCleaner.RemoveNonEssentialCookies(context.HttpContext);
        }
    }

    public DfeCookie GetCookie()
    {
        var cookie = context.HttpContext?.Request.Cookies[Cookie_Key];
        if (cookie is null)
        {
            return new DfeCookie();
        }

        var dfeCookie = JsonSerializer.Deserialize<DfeCookie>(cookie);
        return dfeCookie;
    }

    private void DeleteCookie()
    {
        context.HttpContext?.Response.Cookies.Delete(Cookie_Key);
    }

    private void CreateCookie(string key, bool? userAcceptsCookies = null, bool? visibility = null)
    {
        CookieOptions cookieOptions = new()
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
        context.HttpContext?.Response.Cookies.Append(key, serializedCookie, cookieOptions);
        _dfeCookie = cookie;
    }
}
