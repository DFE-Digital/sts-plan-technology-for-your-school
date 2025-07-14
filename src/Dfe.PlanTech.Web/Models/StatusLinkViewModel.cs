using Dfe.PlanTech.Domain.CategoryLanding;

namespace Dfe.PlanTech.Web.Models
{
    public class StatusLinkViewModel
    {
        public CategoryLandingSection Section { get; set; } = null!;

        public string Context { get; set; } = null!;
    }
}
