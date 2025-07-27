namespace Dfe.PlanTech.Application.Configuration;

public record ApiAuthenticationConfiguration
{
    public string KeyValue { get; init; } = "";

    public bool HaveApiKey => !string.IsNullOrEmpty(KeyValue);

    public bool ApiKeyMatches(string key) => KeyValue.Equals(key);
}
