using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Helpers;

namespace Dfe.PlanTech.Web.ViewModels;

public class CategoryLandingSectionViewModel
{
    public string? DateUpdated { get; init; } = null!;
    public string? ErrorMessage { get; init; }
    public string? LastCompletionDate { get; init; } = null;
    public string Name { get; init; }
    public string? TrustName { get; init; } = string.Empty;
    public SectionProgressStatus ProgressStatus { get; init; }
    public CategoryLandingSectionRecommendationsViewModel Recommendations { get; init; } = null!;
    public string ShortDescription { get; init; }
    public string? Slug { get; init; }

    public CategoryLandingSectionViewModel(
        QuestionnaireSectionEntry section,
        CategoryLandingSectionRecommendationsViewModel recommendations,
        SqlSectionStatusDto? sectionStatus,
        bool hadRetrievalError
    )
    {
        Slug = section.InterstitialPage?.Slug;
        Name = section.Name;
        ShortDescription = section.ShortDescription;
        Recommendations = recommendations;
        TrustName = sectionStatus?.TrustName ?? "a school";

        if (sectionStatus is not null)
        {
            DateUpdated = DateTimeHelper.FormattedDateShort(sectionStatus.DateUpdated);
            LastCompletionDate = sectionStatus.LastCompletionDate is null
                ? string.Empty
                : DateTimeHelper.FormattedDateShort(sectionStatus.LastCompletionDate.Value);
        }

        if (string.IsNullOrWhiteSpace(Slug))
        {
            ErrorMessage = $"{Name} at {Slug} unavailable";
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
        var completed = started && sectionStatus?.LastCompletionDate is not null;

        if (hadRetrievalError)
        {
            return SectionProgressStatus.RetrievalError;
        }
        else if (completed)
        {
            return SectionProgressStatus.Completed;
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
