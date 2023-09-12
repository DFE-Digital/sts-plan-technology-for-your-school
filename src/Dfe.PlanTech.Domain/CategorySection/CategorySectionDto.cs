namespace Dfe.PlanTech.Domain.CategorySection;


public class CategorySectionDto
{
    public string? Slug { get; set; }

    public string Name { get; init; } = null!;

    public string TagColour { get; set; } = null!;

    public string? TagText { get; set; }

    public string? NoSlugForSubtopicErrorMessage { get; set; }

    public bool IsComplete => TagText == SectionConstants.COMPLETE_TEXT;
}

