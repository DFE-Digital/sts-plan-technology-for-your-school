using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Application.Configuration;

[ExcludeFromCodeCoverage]
public record GoogleTagManagerConfiguration
{
    public string Id { get; set; } = "";
    public string SiteVerificationId { get; init; } = "";
}
