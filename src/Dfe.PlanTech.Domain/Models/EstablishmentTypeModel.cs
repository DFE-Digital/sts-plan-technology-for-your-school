using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Domain.Models;

public class EstablishmentTypeModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
}
