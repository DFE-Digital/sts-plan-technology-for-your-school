using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Dfe.PlanTech.Core.Exceptions;

namespace Dfe.PlanTech.Core.Models;

[ExcludeFromCodeCoverage]
public sealed class EstablishmentModel
{
    public const string InvalidEstablishmentErrorMessage =
        $"{nameof(Urn)}, {nameof(Ukprn)}, and {nameof(Uid)} are all invalid";

    [JsonPropertyName("category")]
    public IdWithNameModel? Category { get; set; }

    [JsonPropertyName("DistrictAdministrative_code")]
    public string DistrictAdministrativeCode { get; set; } = null!;

    [JsonPropertyName("groupUid")]
    public string? GroupUid { get; set; }

    public string LegacyId { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("sid")]
    public string Sid { get; set; } = null!;

    [JsonPropertyName("type")]
    public IdWithNameModel? Type { get; set; }

    [JsonPropertyName("uid")]
    public string? Uid { get; set; }

    [JsonPropertyName("ukprn")]
    public string? Ukprn { get; set; }

    [JsonPropertyName("urn")]
    public string? Urn { get; set; }

    public bool IsValid => References.Any(reference => !string.IsNullOrEmpty(reference));

    public string Reference =>
        References.FirstOrDefault(reference => !string.IsNullOrEmpty(reference))
        ?? throw new InvalidEstablishmentException(InvalidEstablishmentErrorMessage);

    private IEnumerable<string?> References
    {
        get
        {
            yield return Urn;
            yield return Ukprn;
            yield return Uid;
        }
    }
}
