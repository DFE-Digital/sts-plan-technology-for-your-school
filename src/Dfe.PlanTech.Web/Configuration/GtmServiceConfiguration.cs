using Dfe.PlanTech.Domain.Cookie.Interfaces;

namespace Dfe.PlanTech.Web.Configuration;

public record GtmServiceConfiguration(GtmConfiguration config, ICookieService cookies)
{
    public GtmConfiguration Config { get; } = config;
    public ICookieService Cookies { get; } = cookies;
}
