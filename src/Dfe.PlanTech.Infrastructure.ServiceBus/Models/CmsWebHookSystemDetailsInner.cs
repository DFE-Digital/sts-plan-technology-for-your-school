using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Infrastructure.ServiceBus.Models;

[ExcludeFromCodeCoverage]
public class CmsWebHookSystemDetailsInner
{
    public string Id { get; init; } = null!;
    public string LinkType { get; init; } = null!;
    public string Type { get; init; } = null!;
}
