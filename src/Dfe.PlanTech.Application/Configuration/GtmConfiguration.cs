namespace Dfe.PlanTech.Application.Configuration;

public record GtmConfiguration
{
    public string Id { get; set; } = "";
    public string SiteVerificationId { get; init; } = "";
}
