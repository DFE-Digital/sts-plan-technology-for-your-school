using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Extensions;
using Dfe.PlanTech.Core.Helpers;

namespace Dfe.PlanTech.Web.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class MatRecommendationSchoolViewModel
    {
        public int EstablishmentId { get; init; }
        public string SchoolName { get; init; } = string.Empty;
        public RecommendationStatus? Status { get; init; }
        public DateTime? LastUpdated { get; init; }
        public bool RequiresSelfAssessment { get; init; }

        public string StatusText => Status?.GetDisplayName() ?? string.Empty;

        public string StatusTagClass =>
            Status.GetCssClassOrDefault(
                RecommendationConstants.DefaultTagClass
            );

        public string LastUpdatedFormatted =>
            LastUpdated?.ToString("d MMMM yyyy")
            ?? RecommendationConstants.DefaultLastUpdatedText;
    }
}
