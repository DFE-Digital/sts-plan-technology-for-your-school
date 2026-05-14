using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Configuration;

[ExcludeFromCodeCoverage]
public record ErrorPagesConfiguration
{
    public string InternalErrorPageId { get; set; } = "";
}
