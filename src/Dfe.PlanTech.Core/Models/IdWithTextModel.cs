using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Core.RoutingDataModel;

public class IdWithTextModel : IdModel
{
    [JsonPropertyName("text")]
    public string Text { get; init; } = null!;
}
