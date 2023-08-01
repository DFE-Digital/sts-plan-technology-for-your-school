using Dfe.PlanTech.Domain.SignIn.Enums;
using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Domain.SignIn.Models;

public sealed class Organisation
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public IdWithName Category { get; set; } = null!;

    public IdWithName Type { get; set; } = null!;

    public string? Urn { get; set; }

    public string? Uid { get; set; }

    public string? Ukprn { get; set; }

    public string LegacyId { get; set; } = null!;

    public string Sid { get; set; } = null!;

    [JsonPropertyName("DistrictAdministrative_code")]
    public string DistrictAdministrativeCode { get; set; } = null!;

    public string Reference => Urn ?? Ukprn ?? Uid ?? Id.ToString();
}

public class IdWithName {
    public string Id { get; init; } = null!;

    public string Name { get; init; } = null!;
}