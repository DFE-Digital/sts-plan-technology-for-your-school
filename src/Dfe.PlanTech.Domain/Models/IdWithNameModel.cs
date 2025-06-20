using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Domain.Models;

public class IdWithNameModel
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;
}
