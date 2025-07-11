using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Core.RoutingDataModel;

public class IdWithNameModel : IdModel
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;
}
