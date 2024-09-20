using Dfe.PlanTech.Domain.Cookie.Interfaces;

namespace Dfe.PlanTech.Web.Helpers;

public class GtmService(GtmConfiguration config, ICookieService cookies)
{
  public GtmConfiguration Config { get; } = config;
  public ICookieService Cookies { get; } = cookies;
}
