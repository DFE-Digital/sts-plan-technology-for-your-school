using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Web.Models
{
    public class CategoryLandingPageViewModel
    {
        public string Slug { get; set; } = null!;

        public Title Title { get; set; } = null!;

        public Category Category { get; set; } = null!;

        public string? SectionName { get; set; }
    }
}
