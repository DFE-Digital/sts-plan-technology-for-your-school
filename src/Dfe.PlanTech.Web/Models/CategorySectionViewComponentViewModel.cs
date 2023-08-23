using Dfe.PlanTech.Domain.CategorySection;

namespace Dfe.PlanTech.Web.Models
{
    public class CategorySectionViewComponentViewModel
    {
        public int CompletedSectionCount { get; init; }

        public int TotalSectionCount { get; init; }

        public IEnumerable<CategorySectionDto> CategorySectionDto { get; init; } = null!;
    }
}