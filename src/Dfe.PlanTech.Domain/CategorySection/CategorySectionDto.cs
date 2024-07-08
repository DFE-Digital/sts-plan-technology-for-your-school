using Dfe.PlanTech.Domain.Constants;
using Dfe.PlanTech.Domain.Helpers;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.CategorySection;

public class CategorySectionDto
{
    public string? Slug { get; init; }

    public string Name { get; init; }

    public Tag Tag { get; init; }

    public string? ErrorMessage { get; init; }

    public CategorySectionRecommendationDto? Recommendation { get; init; }

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
        var lastEdited = LastEditedDate(sectionStatus?.DateUpdated, systemTime);
        if (string.IsNullOrWhiteSpace(slug))
        {
            ErrorMessage = $"{Name} unavailable";
            Tag = new Tag();
        }
        else if (retrievalError)
            Tag = new Tag("unable to retrieve status", TagColour.Red);
        else if (completed)
            Tag = new Tag($"last completed {lastEdited}", TagColour.Grey);
        else if (started)
            Tag = new Tag($"in progress {lastEdited}", TagColour.Grey);
        else
            Tag = new Tag("not started", TagColour.Grey);
    }

    private static string? LastEditedDate(DateTime? date, ISystemTime systemTime)
    {
        if (date == null)
            return null;
        var britishTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(date.Value, britishTimeZone);
        return localTime.Date == systemTime.Today.Date ? $"{localTime:h:mmtt}".ToLower() : $"{localTime:d MMM yyyy}";
    }
}