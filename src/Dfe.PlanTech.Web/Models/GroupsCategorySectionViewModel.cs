using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;

namespace Dfe.PlanTech.Web.Models;

public class GroupsCategorySectionViewModel
{
    public string? ErrorMessage { get; init; }
    public string Name { get; init; } = null!;
    public CategorySectionRecommendationViewModel? Recommendation { get; init; }
    public string? RecommendationSlug { get; init; }
    public string? Slug { get; init; }
    public CmsTagDto Tag { get; init; }

    public GroupsCategorySectionViewModel(
        CmsQuestionnaireSectionDto section,
        CategorySectionRecommendationViewModel recommendation,
        SqlSectionStatusDto? sectionStatus,
        bool hadRetrievalError
    )
    {
        Name = section.Name;
        Recommendation = recommendation;
        RecommendationSlug = string.IsNullOrEmpty(recommendation.SectionSlug)
            ? string.Empty
            : $"{UrlConstants.GroupsSlug}/recommendations/{recommendation.SectionSlug}";
        Slug = section.InterstitialPage?.Slug;

        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = $"{Name} unavailable";
            Tag = new CmsTagDto();
        }
        Tag = GetGroupSubmissionStatusTag(sectionStatus, hadRetrievalError);
    }

    private CmsTagDto GetGroupSubmissionStatusTag(SqlSectionStatusDto? sectionStatus, bool hadRetrievalError)
    {
        var currentCompleted = sectionStatus?.Completed == true;
        var lastCompleted = LastEditedDate(sectionStatus?.LastCompletionDate);
        var lastEdited = LastEditedDate(sectionStatus?.DateUpdated);
        var previouslyCompleted = lastCompleted is not null;

        string message;
        string colour;

        if (hadRetrievalError)
        {
            message = "Unable to retrieve status";
            colour = "Red";
        }
        else if (previouslyCompleted)
        {
            message = $"Completed {lastCompleted}";
            colour = "Blue";
        }
        else if (currentCompleted)
        {
            message = $"Completed {lastEdited}";
            colour = "Blue";
        }
        else
        {
            message = "Not started";
            colour = "Grey";
        }

        return new CmsTagDto($"{message}", TagColourHelper.GetMatchingColour(colour));
    }

    private static string? LastEditedDate(DateTime? date)
    {
        if (date is null)
        {
            return null;
        }

        var localTime = TimeZoneHelper.ToUkTime(date.Value);
        var now = TimeZoneHelper.ToUkTime(DateTime.UtcNow);
        return localTime.Date.Equals(now.Date)
            ? $"at {DateTimeHelper.FormattedTime(localTime)}"
            : $"on {DateTimeHelper.FormattedDateShort(localTime)}";
    }
}
