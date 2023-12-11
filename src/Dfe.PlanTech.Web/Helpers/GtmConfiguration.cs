using Dfe.PlanTech.Domain.Cookie.Interfaces;

namespace Dfe.PlanTech.Web.Helpers;

public sealed class GtmConfiguration
{
    private const string CONFIG_KEY = "GTM";
    private readonly ICookieService _cookieService;

    private readonly string _body;
    private readonly string _head;
    private readonly string _analytics;

    public GtmConfiguration(ICookieService cookieService, IConfiguration configuration)
    {
        _cookieService = cookieService;
        _body = configuration.GetValue<string>($"{CONFIG_KEY}:Body") ?? "";
        _head = configuration.GetValue<string>($"{CONFIG_KEY}:Head") ?? "";
        _analytics = configuration.GetValue<string>($"{CONFIG_KEY}:Analytics") ?? "";
    }
    public string Body => _cookieService.GetCookie().HasApproved ? _body : "";
    public string Head => _cookieService.GetCookie().HasApproved ? _head : "";
    public string Analytics => _analytics;
}
