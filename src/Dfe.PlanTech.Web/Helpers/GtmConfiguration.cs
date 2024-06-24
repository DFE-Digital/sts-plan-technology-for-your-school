using Dfe.PlanTech.Domain.Cookie;
using Dfe.PlanTech.Domain.Cookie.Interfaces;

namespace Dfe.PlanTech.Web.Helpers;

public sealed class GtmConfiguration(ICookieService cookieService, IConfiguration configuration)
{
    private const string CONFIG_KEY = "GTM";

    private readonly string _body = configuration.GetValue<string>($"{CONFIG_KEY}:Body") ?? "";
    private readonly string _head = configuration.GetValue<string>($"{CONFIG_KEY}:Head") ?? "";
    private readonly string _analytics = configuration.GetValue<string>($"{CONFIG_KEY}:Analytics") ?? "";

    private readonly DfeCookie _cookie = cookieService.Cookie;

    public string Body => _cookie.UseCookies ? _body : "";
    public string Head => _cookie.UseCookies ? _head : "";
    public string Analytics => _analytics;
}
