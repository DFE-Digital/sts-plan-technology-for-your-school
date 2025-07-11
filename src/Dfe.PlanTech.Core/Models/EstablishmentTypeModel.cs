using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Core.RoutingDataModel;

public class EstablishmentTypeModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
}
