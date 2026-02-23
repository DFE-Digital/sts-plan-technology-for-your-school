using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Configuration;

[ExcludeFromCodeCoverage]
public record ContactOptionsConfiguration
{
    public string LinkId { get; set; } = "";
}
