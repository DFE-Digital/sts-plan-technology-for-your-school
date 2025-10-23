using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Web.ViewModels;

public class CategorySectionViewModel
{
    public string? ErrorMessage { get; init; }
    public string Name { get; init; } = null!;
    public SubmissionStatus ProgressStatus { get; init; }
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
            ProgressStatus = SubmissionStatus.RetrievalError;
        }
        else
        {
            ProgressStatus = GetSectionSubmissionStatus(sectionStatus, hadRetrievalError);
        }
    }

    private SubmissionStatus GetSectionSubmissionStatus(SqlSectionStatusDto? sectionStatus, bool hadRetrievalError)
    {
        var started = sectionStatus is not null;
        var completed = started && sectionStatus!.Completed;

        if (hadRetrievalError)
        {
            return SubmissionStatus.RetrievalError;
        }
        else if (completed)
        {
            return SubmissionStatus.CompleteReviewed;
        }
        else if (started)
        {
            return SubmissionStatus.InProgress;
        }
        else
        {
            return SubmissionStatus.NotStarted;
        }
    }
}
