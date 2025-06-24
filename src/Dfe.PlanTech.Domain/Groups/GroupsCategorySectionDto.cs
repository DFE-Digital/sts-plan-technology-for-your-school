using Dfe.PlanTech.Domain.CategorySection;
using Dfe.PlanTech.Domain.Helpers;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.Groups;

public class GroupsCategorySectionDto
{
    public string? Slug { get; init; }

    public string? RecommendationSlug { get; init; }

    public string Name { get; init; }

    public Tag Tag { get; init; }

    public string? ErrorMessage { get; init; }

    public CategorySectionRecommendationDto? Recommendation { get; init; }

    public GroupsCategorySectionDto(
        string? slug,
        string? recommendationSlug,
        string name,
        bool retrievalError,
        CategorySectionRecommendationDto? recommendation,
        SectionStatusDto? sectionStatus,
        ISystemTime systemTime)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            ErrorMessage = $"{name} unavailable";
            Tag = new Tag();
        }
        var tag = SubmissionStatusHelpers.GetGroupsSubmissionStatusTag(retrievalError, sectionStatus, systemTime);

        Slug = slug;
        RecommendationSlug = recommendationSlug;
        Name = name;
        Tag = tag;
        Recommendation = recommendation;
    }
}
