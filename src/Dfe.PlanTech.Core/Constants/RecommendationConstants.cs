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
    public static readonly List<string> ValidStatusKeys = new()
    {
        NotStartedKey,
        InProgressKey,
        CompletedKey
    };

    // Status display mapping for consistent UI display
    public static readonly Dictionary<string, string> StatusDisplayNames = new()
    {
        [NotStartedKey] = "Not started",
        [InProgressKey] = "In progress",
        [CompletedKey] = "Completed"
    };

    // Replace spaces with non-breaking spaces, to avoid word-wrap
    // Note: Using the character code and not HTML entity (&nbsp;) to avoid Html.Raw in Razor views
    public static Dictionary<string, string> StatusDisplayNamesNonBreakingSpaces => StatusDisplayNames
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Replace(" ", "\u00A0"));

    // GOV.UK Design System (GDS) status tag CSS classes
    public static readonly Dictionary<string, string> StatusTagClasses = new()
    {
        [NotStartedKey] = "govuk-tag--grey",
        [InProgressKey] = "govuk-tag--blue",
        [CompletedKey] = "govuk-tag--green"
    };

    // Default tag class for unknown statuses
    public const string DefaultTagClass = "govuk-tag--grey";
}
