namespace Dfe.PlanTech.Web.Configurations;

public record GtmConfiguration
{
    public string Id { get; set; } = "";
    public string SiteVerificationId { get; init; } = "";
}
