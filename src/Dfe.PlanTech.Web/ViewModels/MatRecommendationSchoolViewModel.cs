using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Web.ViewModels
{
    public class MatRecommendationSchoolViewModel
    {
        public int EstablishmentId { get; init; }
        public string SchoolName { get; init; } = string.Empty;
        public RecommendationStatus Status { get; init; }
        public DateTime? LastUpdated { get; init; }
    }
}
