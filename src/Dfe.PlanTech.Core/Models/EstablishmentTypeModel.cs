using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Core.Models;

public class EstablishmentTypeModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
}
