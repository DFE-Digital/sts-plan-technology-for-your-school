using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Configuration;

[ExcludeFromCodeCoverage]
public record ErrorMessagesConfiguration
{
    public string ConcurrentUsersOrContentChange { get; set; } = "";
}
