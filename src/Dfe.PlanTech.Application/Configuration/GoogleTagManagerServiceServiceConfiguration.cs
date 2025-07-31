using Dfe.PlanTech.Application.Services;

namespace Dfe.PlanTech.Application.Configuration;

public record GoogleTagManagerServiceServiceConfiguration(GoogleTagManagerConfiguration config, CookieService cookies)
{
    public GoogleTagManagerConfiguration Config { get; } = config;
    public CookieService Cookies { get; } = cookies;
}
