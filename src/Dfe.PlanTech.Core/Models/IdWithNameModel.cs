using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Core.Models;

public class IdWithNameModel : IdModel
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;
}
