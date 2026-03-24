using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Configuration;

[ExcludeFromCodeCoverage]
public record GovUkNotifyConfiguration
{
    public string ApiKey { get; set; } = "";
}
