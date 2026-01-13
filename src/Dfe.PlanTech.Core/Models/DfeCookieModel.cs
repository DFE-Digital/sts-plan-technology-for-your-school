using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Core.Models;

public readonly record struct DfeCookieModel
{
    public DfeCookieModel() { }

    public bool IsVisible { get; init; } = true;
    public bool? UserAcceptsCookies { get; init; } = null;

    [JsonIgnore]
    public bool UseCookies => UserAcceptsCookies ?? false;

    [JsonIgnore]
    public bool UserPreferencesSet => UserAcceptsCookies != null;
}
