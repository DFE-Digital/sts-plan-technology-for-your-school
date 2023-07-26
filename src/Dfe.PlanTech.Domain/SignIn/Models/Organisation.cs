using Dfe.PlanTech.Domain.SignIn.Enums;
using System.Text.Json.Serialization;

namespace Dfe.PlanTech.Domain.SignIn.Models;

public sealed class Organisation
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public IdentityTag<OrganisationCategory> Category { get; set; } = null!;

    public IdentityTag<EstablishmentType> Type { get; set; } = null!;

    public string Urn { get; set; } = null!;

    public string Uid { get; set; } = null!;

    public string Ukprn { get; set; } = null!;

    public string LegacyId { get; set; } = null!;

    public string Sid { get; set; } = null!;

    [JsonPropertyName("DistrictAdministrative_code")]
    public string DistrictAdministrativeCode { get; set; } = null!;
}