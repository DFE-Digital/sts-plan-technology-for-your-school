using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Web.Models
{
    public class CategorySectionViewModel
    {
        public string? ErrorMessage { get; init; }
        public string Name { get; init; } = null!;
        public SectionProgressStatus ProgressStatus { get; init; }
        public CategorySectionRecommendationViewModel? Recommendation { get; init; }
        public string? Slug { get; init; }

        public CategorySectionViewModel(
            CmsQuestionnaireSectionDto section,
            CategorySectionRecommendationViewModel recommendation,
            SqlSectionStatusDto? sectionStatus,
            bool hadRetrievalError
        )
        { 
            Name = section.Name;
            Recommendation = recommendation;
            Slug = section.InterstitialPage?.Slug;

            var started = sectionStatus is not null;
            var completed = started && sectionStatus!.Completed;
            var startedNeverCompleted = started && !completed && sectionStatus!.LastCompletionDate is null;
            var completedStartedNew = started && !completed && sectionStatus!.LastCompletionDate is not null;

            if (string.IsNullOrWhiteSpace(Slug))
            {
                ErrorMessage = $"Slug {Slug} unavailable";
                ProgressStatus = SectionProgressStatus.RetrievalError;
            }
            else if (hadRetrievalError)
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
    }
}
