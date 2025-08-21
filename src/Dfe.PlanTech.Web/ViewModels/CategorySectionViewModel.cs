using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Web.ViewModels;

public class CategorySectionViewModel
{
    public string? ErrorMessage { get; init; }
    public string Name { get; init; } = null!;
    public SectionProgressStatus ProgressStatus { get; init; }
    public CategorySectionRecommendationViewModel? Recommendation { get; init; }
    public string? Slug { get; init; }

    public CategorySectionViewModel(
        QuestionnaireSectionEntry section,
        CategorySectionRecommendationViewModel recommendation,
        SqlSectionStatusDto? sectionStatus,
        bool hadRetrievalError
    )
    {
        Name = section.Name;
        Recommendation = recommendation;
        Slug = section.InterstitialPage?.Slug;

        if (string.IsNullOrWhiteSpace(Slug))
        {
            ErrorMessage = $"Slug {Slug} unavailable";
            ProgressStatus = SectionProgressStatus.RetrievalError;
        }
        else
        {
            ProgressStatus = GetSectionProgressStatus(sectionStatus, hadRetrievalError);
        }
    }

    private SectionProgressStatus GetSectionProgressStatus(SqlSectionStatusDto? sectionStatus, bool hadRetrievalError)
    {
        var started = sectionStatus is not null;
        var completed = started && sectionStatus!.Completed;
        var startedNeverCompleted = started && !completed && sectionStatus!.LastCompletionDate is null;
        var completedStartedNew = started && !completed && sectionStatus!.LastCompletionDate is not null;

        if (hadRetrievalError)
        {
            return SectionProgressStatus.RetrievalError;
        }
        else if (completed)
        {
            return SectionProgressStatus.Completed;
        }
        else if (startedNeverCompleted)
        {
            return SectionProgressStatus.StartedNeverCompleted;
        }
        else if (completedStartedNew)
        {
            return SectionProgressStatus.CompletedStartedNew;
        }
        else if (started)
        {
            return SectionProgressStatus.InProgress;
        }
        else
        {
            return SectionProgressStatus.NotStarted;
        }
    }
}
