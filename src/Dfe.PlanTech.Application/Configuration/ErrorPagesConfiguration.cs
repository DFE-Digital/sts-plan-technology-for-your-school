using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Application.Configuration;

[ExcludeFromCodeCoverage]
public record ErrorPagesConfiguration
{
    public string InternalErrorPageId { get; set; } = "";
}
