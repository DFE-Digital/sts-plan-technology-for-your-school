using System.Text.Json;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Dfe.PlanTech.Application.Services;

public class CookieService(IHttpContextAccessor contextAccessor, ICookieWorkflow cookieWorkflow)
    : ICookieService
{
    private readonly IHttpContextAccessor _contextAccessor =
        contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
    private readonly ICookieWorkflow _cookieWorkflow =
        cookieWorkflow ?? throw new ArgumentNullException(nameof(cookieWorkflow));

    private DfeCookieModel? _dfeCookie;
    public DfeCookieModel Cookie => _dfeCookie ?? GetCookie();

    public void SetVisibility(bool visibility)
    {
        CreateCookie(CookieConstants.UserCookiesPreferences, visibility: visibility);
    }

    public void SetCookieAcceptance(bool userAcceptsCookies)
    {
        if (_contextAccessor.HttpContext is null)
        {
            throw new InvalidOperationException(
                $"Cannot set cookie acceptance as {nameof(_contextAccessor.HttpContext)} is null"
            );
        }

        CreateCookie(
            CookieConstants.UserCookiesPreferences,
            userAcceptsCookies: userAcceptsCookies
        );

        if (!userAcceptsCookies)
        {
            _cookieWorkflow.RemoveNonEssentialCookies(_contextAccessor.HttpContext);
        }
    }

    public DfeCookieModel GetCookie()
    {
        var cookie = _contextAccessor
            .HttpContext
            ?.Request
            .Cookies[CookieConstants.UserCookiesPreferences];
        if (cookie is null)
        {
            return new DfeCookieModel();
        }

        var dfeCookie = JsonSerializer.Deserialize<DfeCookieModel>(cookie);
        return dfeCookie;
    }

    private void DeleteCookie()
    {
        _contextAccessor.HttpContext?.Response.Cookies.Delete(
            CookieConstants.UserCookiesPreferences
        );
    }

    private void CreateCookie(string key, bool? userAcceptsCookies = null, bool? visibility = null)
    {
        CookieOptions cookieOptions = new()
        {
            Secure = true,
            HttpOnly = true,
            Expires = new DateTimeOffset(DateTime.Now.AddYears(1)),
        };

        DeleteCookie();

        var cookie = Cookie with
        {
            UserAcceptsCookies = userAcceptsCookies ?? Cookie.UserAcceptsCookies,
            IsVisible = visibility ?? Cookie.IsVisible,
        };

        var serializedCookie = JsonSerializer.Serialize(cookie);
        _contextAccessor.HttpContext?.Response.Cookies.Append(key, serializedCookie, cookieOptions);
        _dfeCookie = cookie;
    }
}
