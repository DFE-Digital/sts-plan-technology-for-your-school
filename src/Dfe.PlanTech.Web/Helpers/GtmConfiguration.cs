using Dfe.PlanTech.Application.Cookie.Interfaces;

namespace Dfe.PlanTech.Web.Helpers;

public sealed class GtmConfiguration
{
    private const string CONFIG_KEY = "GTM";
    private readonly ICookieService _cookieService;

    private readonly string _body;
    private readonly string _head;

    public GtmConfiguration(ICookieService cookieService, IConfiguration configuration)
    {
        _cookieService = cookieService;
        _body = configuration.GetValue<string>($"{CONFIG_KEY}:Body") ?? "";
        _head = configuration.GetValue<string>($"{CONFIG_KEY}:Head") ?? "";
    }
    public string Body => _cookieService.GetCookie().HasApproved ? _body : "";
    public string Head => _cookieService.GetCookie().HasApproved ? _head : "";
}
