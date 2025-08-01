
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Web.Models
{
    public class HeaderContentViewModel
    {
        public IHeaderWithContent? Header { get; set; }
        public string? SubmissionDate { get; set; }
        public string? SectionName { get; set; }

        public int? RecommendationCount { get; set; }

        public int CurrentRecommendationPageCount { get; set; }
    }
}
