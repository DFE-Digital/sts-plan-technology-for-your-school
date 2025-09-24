using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Application.Configuration;

[ExcludeFromCodeCoverage]
public record ErrorMessagesConfiguration
{
    public string ConcurrentUsersOrContentChange { get; set; } = "";
}
