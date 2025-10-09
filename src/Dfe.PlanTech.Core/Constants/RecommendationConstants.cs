using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Constants;

[ExcludeFromCodeCoverage]
public static class RecommendationConstants
{
    // Status constants -- todo: review if this is the right way to do it (enum?)
    public const string Completed = "Completed";
    public const string InProgress = "In Progress";
    public const string NotStarted = "Not Started";

    // Default values
    public const string DefaultStatus = NotStarted;
    public const string DefaultLastUpdatedText = "Not known / never";

    // List of all valid status options for UI components
    public static readonly List<string> ValidStatuses = new()
    {
        NotStarted,
        InProgress,
        Completed
    };

    // Status display mapping for consistent UI display
    public static readonly Dictionary<string, string> StatusDisplayNames = new()
    {
        [NotStarted] = "Not started",
        [InProgress] = "In progress",
        [Completed] = "Completed"
    };

    // GOV.UK Design System status tag CSS classes
    public static readonly Dictionary<string, string> StatusTagClasses = new()
    {
        [NotStarted] = "govuk-tag--grey",
        [InProgress] = "govuk-tag--blue",
        [Completed] = "govuk-tag--green"
    };

    // Default tag class for unknown statuses
    public const string DefaultTagClass = "govuk-tag--grey";
}
