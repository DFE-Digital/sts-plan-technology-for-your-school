using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Domain.Models;

public sealed class OrganisationModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public IdWithName? Category { get; set; }

    public IdWithName? Type { get; set; }

    public string? Urn { get; set; }

    public string? Uid { get; set; }

    public string? Ukprn { get; set; }

    public string LegacyId { get; set; } = null!;

    public string Sid { get; set; } = null!;

    [JsonPropertyName("DistrictAdministrative_code")]
    public string DistrictAdministrativeCode { get; set; } = null!;

    public string Reference => Urn ?? Ukprn ?? Uid ?? Id.ToString();
}

public class IdWithName
{
    public string Id { get; init; } = null!;

    public string Name { get; init; } = null!;
}
