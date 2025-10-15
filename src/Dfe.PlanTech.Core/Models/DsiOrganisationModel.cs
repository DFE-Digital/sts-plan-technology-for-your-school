using System.Text.Json.Serialization;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Exceptions;

namespace Dfe.PlanTech.Core.Models;

public sealed class DsiOrganisationModel
{
    public const string InvalidEstablishmentErrorMessage = $"{nameof(Urn)}, {nameof(Ukprn)}, {nameof(Uid)}, and {nameof(Id)} are all invalid";

    [JsonPropertyName("id")]
    public Guid Id { get; set; }

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

    public bool IsGroup()
    {
        if (!string.IsNullOrEmpty(Urn))
        {
            // If there's a URN, this organisation is an establishment
            return false;
        }
        else if (!string.IsNullOrEmpty(Uid))
        {
            // If there's a UID, this organisation is a group
            return true;
        }
        else if (Category?.Id == DsiConstants.MatOrganisationCategoryId)
        {
            // If there's not a UID (checks above) but we do have a known-group category, this organisation is a group
            return true;
        }
        else if (Type is not null)
        {
            // This is an "establishment type" therefore, if it's present, the organisation is an establishment (and, therefore, _not_ a group)
            return false;
        }
        else
        {
            // TODO: What to default to if unable to determine? Yes/No/Unknown
            return true;
        }
    }


    public bool IsValid => References.Any(reference => !string.IsNullOrEmpty(reference));

    public string Reference => References
        .FirstOrDefault(reference => !string.IsNullOrEmpty(reference))
            ?? throw new InvalidEstablishmentException(InvalidEstablishmentErrorMessage);

    private IEnumerable<string?> References
    {
        get
        {
            yield return Urn;
            yield return Ukprn;
            yield return Uid; // TODO: Consider moving UID above UKPRN, given that UID is the GIAS identifier for establishment groups (potential backwards compatability issue?)
            yield return Id.ToString();
        }
    }
}
