using Dfe.PlanTech.Domain.Helpers;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.CategoryLanding;

public enum SectionProgressStatus
{
    NotStarted,
    InProgress,
    Completed,
    RetrievalError
}

public class CategoryLandingSection
{
    public string? Slug { get; init; }

    public string Name { get; init; }

    public string ShortDescription { get; init; }

    public string? ErrorMessage { get; init; }

    public string? DateUpdated { get; init; } = null!;

    public string? LastCompletionDate { get; init; } = null;

    public CategoryLandingSectionRecommendations Recommendations { get; init; } = null!;

    public SectionProgressStatus ProgressStatus { get; init; }

    public CategoryLandingSection(
        string? slug,
        string name,
        string shortDescription,
        bool retrievalError,
        SectionStatusDto? sectionStatus,
        CategoryLandingSectionRecommendations recommendations)
    {
        Slug = slug;
        Name = name;
        ShortDescription = shortDescription;
        Recommendations = recommendations;

        if (sectionStatus != null)
        {
            LastCompletionDate = sectionStatus.LastCompletionDate != null ? DateTimeFormatter.FormattedDateShort(sectionStatus.LastCompletionDate.Value) : "";
            DateUpdated = DateTimeFormatter.FormattedDateShort(sectionStatus.DateUpdated);
        }

        var started = sectionStatus != null;
        var completed = sectionStatus?.LastCompletionDate != null;

        if (string.IsNullOrWhiteSpace(slug))
        {
            ErrorMessage = $"{Name} unavailable";
            ProgressStatus = SectionProgressStatus.RetrievalError;
        }
        else if (retrievalError)
        {
            ProgressStatus = SectionProgressStatus.RetrievalError;
        }
        else if (completed)
        {
            ProgressStatus = SectionProgressStatus.Completed;
        }
        else if (started)
        {
            ProgressStatus = SectionProgressStatus.InProgress;
        }
        else
        {
            ProgressStatus = SectionProgressStatus.NotStarted;
        }
    }
}
