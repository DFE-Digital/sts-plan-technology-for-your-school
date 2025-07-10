using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsRecommendationIntroDto : CmsEntryDto
    {
        public string Id { get; set; } = null!;
        public string InternalName { get; set; } = null!;
        public string Slug { get; init; } = null!;
        public CmsComponentHeaderDto Header { get; init; } = null!;
        public List<CmsEntryDto> Content { get; init; } = [];
        public string Maturity { get; init; } = null!;

        public CmsRecommendationIntroDto(RecommendationIntroEntry recommendationIntroEntry)
        {
            Id = recommendationIntroEntry.Id;
            InternalName = recommendationIntroEntry.InternalName;
            Slug = recommendationIntroEntry.Slug;
            Header = recommendationIntroEntry.Header.AsDto();
            Content = recommendationIntroEntry.Content.Select(BuildContentDto).ToList();
            Maturity = recommendationIntroEntry.Maturity;
        }
    }
}
