using Dfe.PlanTech.Domain.Constants;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.CategorySection;

public class CategorySectionDto
{
    public string? Slug { get; init; }

    public string Name { get; init; }

    public Tag Tag { get; init; }
    
    public string? ErrorMessage { get; init; }
    
    public CategorySectionRecommendationDto? Recommendation { get; init; }

    public CategorySectionDto(string? slug, string name, bool retrievalError, bool started, bool completed, CategorySectionRecommendationDto recommendation)
    {
        Slug = slug;
        Name = name;
        Recommendation = recommendation;
        if (string.IsNullOrWhiteSpace(slug))
        {
            ErrorMessage = $"{Name} unavailable";
            Tag = new Tag();
        }
        else if (retrievalError)
            Tag = new Tag("UNABLE TO RETRIEVE STATUS", TagColour.Red);
        else if (completed)
            Tag = new Tag("COMPLETE", TagColour.Blue);
        else if (started)
            Tag = new Tag("IN PROGRESS", TagColour.LightBlue );
        else
            Tag = new Tag("NOT STARTED", TagColour.Grey );
    }
}