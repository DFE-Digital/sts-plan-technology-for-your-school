using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Web.Configuration;

[ExcludeFromCodeCoverage]
public record SupportedAssetTypesConfiguration
{
    public string[] ImageTypes { get; set; } = null!;
    public string[] VideoTypes { get; set; } = null!;
}
