using Dfe.PlanTech.Application.Services;

namespace Dfe.PlanTech.Application.Configuration;

public record GtmServiceConfiguration(GtmConfiguration config, CookieService cookies)
{
    public GtmConfiguration Config { get; } = config;
    public CookieService Cookies { get; } = cookies;
}
