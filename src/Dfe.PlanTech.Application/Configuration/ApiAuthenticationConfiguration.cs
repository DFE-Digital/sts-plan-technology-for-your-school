using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Application.Configuration;

[ExcludeFromCodeCoverage]
public record ApiAuthenticationConfiguration
{
    public string KeyValue { get; init; } = "";

    public bool HasApiKey => !string.IsNullOrEmpty(KeyValue);

    public bool ApiKeyMatches(string key) => KeyValue.Equals(key);
}
