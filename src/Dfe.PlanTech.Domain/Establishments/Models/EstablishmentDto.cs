using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Domain.Establishments.Models;

public class EstablishmentDto
{
    [JsonPropertyName("ukprn")]
    public string? Ukprn { get; set; }

    [JsonPropertyName("urn")] public string? Urn { get; set; }

    [JsonPropertyName("type")]
    public EstablishmentTypeDto Type { get; set; } = new EstablishmentTypeDto();

    [JsonPropertyName("name")]
    public string OrgName { get; set; } = null!;

    public bool IsValid => !string.IsNullOrEmpty(OrgName) && string.IsNullOrEmpty(Urn) && string.IsNullOrEmpty(Ukprn);

    public string Reference => Urn ?? Ukprn ?? throw new Exception($"Both {nameof(Urn)} and {nameof(Ukprn)} are invalid");
}