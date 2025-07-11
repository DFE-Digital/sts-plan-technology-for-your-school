using System.Text.Json.Serialization;
using Dfe.PlanTech.Core.Exceptions;

namespace Dfe.PlanTech.Core.RoutingDataModel;

public class EstablishmentModel
{
    public const string InvalidEstablishmentErrorMessage = $"Both {nameof(Urn)} and {nameof(Ukprn)} are invalid";

    [JsonPropertyName("ukprn")]
    public string? Ukprn { get; set; }

    [JsonPropertyName("urn")]
    public string? Urn { get; set; }

    [JsonPropertyName("groupUid")]
    public string? GroupUid { get; set; }

    [JsonPropertyName("name")]
    public string OrgName { get; set; } = null!;

    [JsonPropertyName("type")]
    public EstablishmentTypeModel? Type { get; set; }

    public bool IsValid => References()
        .Any(reference => !string.IsNullOrEmpty(reference));

    public string Reference => References()
        .FirstOrDefault(reference => !string.IsNullOrEmpty(reference))
            ?? throw new InvalidEstablishmentException(InvalidEstablishmentErrorMessage);

    private IEnumerable<string?> References()
    {
        yield return Urn;
        yield return Ukprn;
    }
}
