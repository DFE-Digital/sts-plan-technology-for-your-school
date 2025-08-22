namespace Dfe.PlanTech.Application.Configuration;

public record GoogleTagManagerConfiguration
{
    public string Id { get; set; } = "";
    public string SiteVerificationId { get; init; } = "";
}
