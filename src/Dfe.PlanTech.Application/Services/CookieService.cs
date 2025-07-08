using System.Text.Json;
using Dfe.PlanTech.Domain.Cookie;
using Dfe.PlanTech.Application.Workflows;
using Microsoft.AspNetCore.Http;

namespace Dfe.PlanTech.Application.Services;

public class CookieService(
    IHttpContextAccessor contextAccessor,
    CookieWorkflow cookieWorkflow)
{
    private readonly IHttpContextAccessor _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(_contextAccessor));
    private readonly CookieWorkflow _cookieWorkflow = cookieWorkflow ?? throw new ArgumentNullException(nameof(_cookieWorkflow));

    public const string Cookie_Key = "user_cookies_preferences";

    private DfeCookieModel? _dfeCookie;
    public DfeCookieModel Cookie => _dfeCookie ?? GetCookie();

    public void SetVisibility(bool visibility)
    {
        CreateCookie(Cookie_Key, visibility: visibility);
    }

    public void SetCookieAcceptance(bool userAcceptsCookies)
    {
        if (_contextAccessor.HttpContext is null)
        {
            throw new InvalidOperationException($"Cannot set cookie acceptance as {nameof(_contextAccessor.HttpContext)} is null");
        }

        CreateCookie(Cookie_Key, userAcceptsCookies: userAcceptsCookies);

        if (!userAcceptsCookies)
        {
            _cookieWorkflow.RemoveNonEssentialCookies(_contextAccessor.HttpContext);
        }
    }

    public DfeCookieModel GetCookie()
    {
        var cookie = _contextAccessor.HttpContext?.Request.Cookies[Cookie_Key];
        if (cookie is null)
        {
            return new DfeCookieModel();
        }

        var dfeCookie = JsonSerializer.Deserialize<DfeCookieModel>(cookie);
        return dfeCookie;
    }

    private void DeleteCookie()
    {
        _contextAccessor.HttpContext?.Response.Cookies.Delete(Cookie_Key);
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
        _contextAccessor.HttpContext?.Response.Cookies.Append(key, serializedCookie, cookieOptions);
        _dfeCookie = cookie;
    }
}
