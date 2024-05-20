namespace Dfe.PlanTech.Domain.CategorySection;

public class CategorySectionDto
{
    public string? Slug { get; init; }

    public string Name { get; init; }

    public string? TagColour { get; init; }

    public string? TagText { get; init; }

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
        }
        else if (retrievalError)
        {
            TagColour = Constants.TagColour.Red;
            TagText = "UNABLE TO RETRIEVE STATUS";
        }
        else if (completed)
        {
            TagColour = Constants.TagColour.Blue;
            TagText = "COMPLETE";
        }
        else if (started)
        {
            TagColour = Constants.TagColour.LightBlue;
            TagText = "IN PROGRESS";
        }
        else
        {
            TagColour = Constants.TagColour.Grey;
            TagText = "NOT STARTED";
        }
    }
}