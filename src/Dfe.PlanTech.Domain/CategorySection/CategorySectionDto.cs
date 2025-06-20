using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;
using Dfe.PlanTech.Domain.Helpers;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.CategorySection;

//Could be moved to another folder in future but only declared here for now
public enum SectionProgressStatus
{
    NotStarted,
    InProgress,
    StartedNeverCompleted,
    CompletedStartedNew,
    Completed,
    RetrievalError
}

public class CategorySectionDto
{
    public string? Slug { get; init; }

    public string Name { get; init; }

    public Tag Tag { get; init; } = new Tag();

    public string? ErrorMessage { get; init; }

    public CategorySectionRecommendationDto? Recommendation { get; init; }

    public SectionProgressStatus ProgressStatus { get; init; }

    public CategorySectionDto(
        string? slug,
        string name,
        bool retrievalError,
        SectionStatusDto? sectionStatus,
        CategorySectionRecommendationDto recommendation,
        ISystemTime systemTime)
    {
        Slug = slug;
        Name = name;
        Recommendation = recommendation;
        var started = sectionStatus != null;
        var completed = sectionStatus?.Completed == true;
        var completedStartedNew = sectionStatus?.LastCompletionDate != null && sectionStatus?.Completed == false && started;
        var startedNeverCompleted = sectionStatus?.LastCompletionDate == null && sectionStatus?.Completed == false && started;

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
        else if (startedNeverCompleted)
        {
            ProgressStatus = SectionProgressStatus.StartedNeverCompleted;
        }
        else if (completedStartedNew)
        {
            ProgressStatus = SectionProgressStatus.CompletedStartedNew;
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

    private static string? LastEditedDate(DateTime? date, ISystemTime systemTime)
    {
        if (date == null)
            return null;
        var localTime = TimeZoneHelpers.ToUkTime(date.Value);
        return DateTimeHelper.FormattedDateShort(localTime);
    }
}
