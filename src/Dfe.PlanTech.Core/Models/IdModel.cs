using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Core.RoutingDataModel;

public class IdModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;
}
