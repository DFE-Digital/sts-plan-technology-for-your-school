using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsRecommendationPageDto : CmsEntryDto
    {
        public string InternalName { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string Maturity { get; init; } = null!;
        public CmsPageDto Page { get; set; } = null!;

        public CmsRecommendationPageDto(RecommendationPageEntry recommendationPageEntry)
        {
            InternalName = recommendationPageEntry.InternalName;
            DisplayName = recommendationPageEntry.DisplayName;
            Maturity = recommendationPageEntry.Maturity;
            Page = recommendationPageEntry.Page.AsDto();
        }
    }
}
