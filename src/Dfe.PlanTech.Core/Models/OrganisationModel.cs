using System.Text.Json.Serialization;
using Dfe.PlanTech.Core.Exceptions;

namespace Dfe.PlanTech.Core.Models;

public sealed class OrganisationModel
{
    public const string InvalidEstablishmentErrorMessage = $"Both {nameof(Urn)} and {nameof(Ukprn)} are invalid";

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

    public bool IsValid => References.Any(reference => !string.IsNullOrWhiteSpace(reference));

    public string Reference => References.FirstOrDefault(reference => !string.IsNullOrWhiteSpace(reference))
        ?? throw new InvalidEstablishmentException(InvalidEstablishmentErrorMessage);

    private IEnumerable<string?> References
    {
        get
        {
            yield return Urn;
            yield return Ukprn;
        }
    }
}
