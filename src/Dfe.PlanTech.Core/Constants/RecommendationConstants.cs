using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Constants;

[ExcludeFromCodeCoverage]
public static class RecommendationConstants
{
    // Status constants
    // Note that these keys must be simple strings without special characters
    public const string CompletedKey = "Completed";
    public const string InProgressKey = "In Progress";
    public const string NotStartedKey = "Not Started";

    // Default values
    public const string DefaultStatus = NotStartedKey;
    public const string DefaultLastUpdatedText = "Not known / never";

    // List of all valid status options for UI components
    public static readonly ImmutableList<string> ValidStatusKeys = ImmutableList.Create(
        NotStartedKey,
        InProgressKey,
        CompletedKey
    );

    // Status display mapping for consistent UI display
    public static readonly ImmutableDictionary<string, string> StatusDisplayNames =
        ImmutableDictionary.CreateRange(new[]
        {
            new KeyValuePair<string, string>(NotStartedKey, "Not started"),
            new KeyValuePair<string, string>(InProgressKey, "In progress"),
            new KeyValuePair<string, string>(CompletedKey, "Completed")
        });

    // Replace spaces with non-breaking spaces, to avoid word-wrap
    // Note: Using the character code and not HTML entity (&nbsp;) to avoid Html.Raw in Razor views
    public static ImmutableDictionary<string, string> StatusDisplayNamesNonBreakingSpaces =>
        StatusDisplayNames.ToImmutableDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Replace(" ", "\u00A0")
        );

    // GOV.UK Design System (GDS) status tag CSS classes
    public static readonly ImmutableDictionary<string, string> StatusTagClasses =
        ImmutableDictionary.CreateRange(new[]
        {
            new KeyValuePair<string, string>(NotStartedKey, "govuk-tag--grey"),
            new KeyValuePair<string, string>(InProgressKey, "govuk-tag--blue"),
            new KeyValuePair<string, string>(CompletedKey, "govuk-tag--green")
        });

    // Default tag class for unknown statuses
    public const string DefaultTagClass = "govuk-tag--grey";
}
