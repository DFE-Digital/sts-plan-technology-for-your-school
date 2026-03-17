using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Application.Services.Interfaces;

namespace Dfe.PlanTech.Application.Configuration;

[ExcludeFromCodeCoverage]
public record GoogleTagManagerServiceConfiguration(
    ICookieService cookieService,
    GoogleTagManagerConfiguration config
)
{
    public ICookieService CookieService { get; } = cookieService;
    public GoogleTagManagerConfiguration Config { get; } = config;
}
