using Dfe.PlanTech.Domain.CategorySection;

namespace Dfe.PlanTech.Web.Models
{
    public class CategorySectionViewComponentViewModel
    {
        public int CompletedCount { get; init; }

        public IEnumerable<CategorySectionDto> CategorySectionDto { get; init; } = null!;
    }
}