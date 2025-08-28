using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Application.Configuration;

[ExcludeFromCodeCoverage]
public record ContactOptionsConfiguration
{
    public string LinkId { get; set; } = "";
}
