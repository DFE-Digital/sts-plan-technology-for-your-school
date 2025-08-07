using Dfe.PlanTech.Domain.CategoryLanding;

namespace Dfe.PlanTech.Web.Models
{
    public class StatusLinkViewModel
    {
        public string CategorySlug { get; set; } = null!;

        public CategoryLandingSection Section { get; set; } = null!;

        public string Context { get; set; } = null!;
    }
}
