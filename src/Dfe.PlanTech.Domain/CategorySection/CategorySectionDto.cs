using Dfe.PlanTech.Domain.Constants;
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

    public CategorySectionDto(string? slug, string name, bool retrievalError, SectionStatusDto? sectionStatus, CategorySectionRecommendationDto recommendation)
    {
        Slug = slug;
        Name = name;
        Recommendation = recommendation;
        var started = sectionStatus != null;
        var completed = sectionStatus?.Completed == true;
        var lastEdited = LastEditedDate(sectionStatus?.DateCreated);
        if (string.IsNullOrWhiteSpace(slug))
        {
            ErrorMessage = $"{Name} unavailable";
            Tag = new Tag();
        }
        else if (retrievalError)
            Tag = new Tag("UNABLE TO RETRIEVE STATUS", TagColour.Red);
        else if (completed)
            Tag = new Tag($"COMPLETE {lastEdited}", TagColour.Grey);
        else if (started)
            Tag = new Tag($"IN PROGRESS {lastEdited}", TagColour.Grey);
        else
            Tag = new Tag("NOT STARTED", TagColour.Grey);
    }

    private static string? LastEditedDate(DateTime? date)
    {
        if (date == null)
            return null;
        var localTime = date.Value.ToLocalTime();
        return localTime.Date == DateTime.Today.Date ? $"{localTime:hh:mm tt}" : localTime.ToShortDateString();
    }
}