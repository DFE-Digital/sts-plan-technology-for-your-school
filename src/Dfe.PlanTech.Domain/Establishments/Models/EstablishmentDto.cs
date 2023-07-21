using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Domain.Establishments.Models;

public class EstablishmentDto
{
    [JsonPropertyName("ukprn")]
    public string EstablishmentRef { get; set; } = null!;

    [JsonPropertyName("type")]
    public EstablishmentTypeDto Type { get; set; } = new EstablishmentTypeDto();

    [JsonPropertyName("name")]
    public string OrgName { get; set; } = null!;
}