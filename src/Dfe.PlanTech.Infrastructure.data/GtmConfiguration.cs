using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Infrastructure.Data;
[ExcludeFromCodeCoverage]
public sealed class GtmConfiguration
{
    public string Head { get; set; } = null!;
    public string Body { get; set; } = null!;
}
