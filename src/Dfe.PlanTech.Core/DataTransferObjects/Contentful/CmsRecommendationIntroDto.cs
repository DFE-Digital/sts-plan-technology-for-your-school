using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Extensions;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsRecommendationIntroDto : CmsEntryDto, IHeaderWithContent
{
    public string Id { get; set; } = null!;
    public string InternalName { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public CmsComponentHeaderDto Header { get; set; } = null!;
    public List<CmsEntryDto> Content { get; set; } = [];
    public string Maturity { get; set; } = null!;

    public string HeaderText => Header.Text;
    public string LinkText => "Overview";
    public string SlugifiedLinkText => LinkText.Slugify();

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
