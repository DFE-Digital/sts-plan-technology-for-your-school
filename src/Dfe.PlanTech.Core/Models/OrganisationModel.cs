using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Core.Models;

public sealed class OrganisationModel
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("category")]
    public IdWithNameModel? Category { get; set; }

    public IdWithNameModel? Type { get; set; }

    public string? Urn { get; set; }

    public string? Uid { get; set; }

    public string? Ukprn { get; set; }

    public string LegacyId { get; set; } = null!;

    public string Sid { get; set; } = null!;

    [JsonPropertyName("DistrictAdministrative_code")]
    public string DistrictAdministrativeCode { get; set; } = null!;

    public string Reference => Urn ?? Ukprn ?? Uid ?? Id.ToString();
}
