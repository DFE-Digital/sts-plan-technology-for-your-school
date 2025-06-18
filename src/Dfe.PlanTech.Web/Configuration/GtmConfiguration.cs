namespace Dfe.PlanTech.Web.Configuration;

public record GtmConfiguration
{
    public string Id { get; set; } = "";
    public string SiteVerificationId { get; init; } = "";
}
