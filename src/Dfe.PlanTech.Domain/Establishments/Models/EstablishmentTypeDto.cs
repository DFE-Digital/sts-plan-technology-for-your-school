using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Domain.Establishments.Models;

public class EstablishmentTypeDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
}
