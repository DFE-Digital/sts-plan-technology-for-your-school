using Dfe.PlanTech.Domain.CategoryLanding;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Web.Models
{
    public class CategoryLandingViewComponentViewModel
    {
        public string CategoryName { get; set; } = null!;

        public List<Section> Sections { get; set; } = [];

        public List<CategoryLandingSection> CategoryLandingSections { get; init; } = null!;

        public bool AllSectionsCompleted { get; init; }

        public bool AnySectionsCompleted { get; init; }

        public string? NoSectionsErrorRedirectUrl { get; set; }

        public string? ProgressRetrievalErrorMessage { get; set; }
    }
}
