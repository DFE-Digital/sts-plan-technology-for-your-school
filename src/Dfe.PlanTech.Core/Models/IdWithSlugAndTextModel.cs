using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Core.RoutingDataModel;

public class IdWithSlugAndTextModel : IdWithTextModel
{
    [JsonPropertyName("slug")]
    public string Slug { get; init; } = null!;
}
