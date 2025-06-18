using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Web.Configuration;

[ExcludeFromCodeCoverage]
public record TrackingOptionsConfiguration
{
    public string Gtm { get; set; } = null!;
    public string Clarity { get; set; } = null!;
}
