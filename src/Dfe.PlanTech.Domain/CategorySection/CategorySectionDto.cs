namespace Dfe.PlanTech.Domain.CategorySection
{
    public class CategorySectionDto
    {
        public string? Slug { get; init; }

        public string? Name { get; init; }

        public string TagColour { get; set; } = null!;

        public string TagText { get; set; } = null!;
    }
}