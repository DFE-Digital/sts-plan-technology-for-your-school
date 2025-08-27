using Dfe.PlanTech.Application.Services.Interfaces;

namespace Dfe.PlanTech.Application.Configuration;

public record GoogleTagManagerServiceServiceConfiguration(ICookieService cookieService, GoogleTagManagerConfiguration config)
{
    public ICookieService CookieService { get; } = cookieService;
    public GoogleTagManagerConfiguration Config { get; } = config;
}
