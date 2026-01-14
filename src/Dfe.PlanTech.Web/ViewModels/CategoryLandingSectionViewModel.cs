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
    public SubmissionStatus ProgressStatus { get; init; }
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
            ProgressStatus = SubmissionStatus.RetrievalError;
        }
        else
        {
            ProgressStatus = hadRetrievalError
                 ? SubmissionStatus.RetrievalError
                 : sectionStatus?.Status ?? SubmissionStatus.NotStarted;
        }
    }
}
