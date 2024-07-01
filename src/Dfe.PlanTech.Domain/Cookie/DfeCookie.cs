using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Domain.Cookie;

public readonly record struct DfeCookie
{
    public DfeCookie()
    {
    }

    public bool? UserAcceptsCookies { get; init; } = null;

    public bool IsVisible { get; init; } = true;

    [JsonIgnore]
    public bool UseCookies => UserAcceptsCookies ?? false;

    [JsonIgnore]
    public bool UserPreferencesSet => UserAcceptsCookies != null;
}