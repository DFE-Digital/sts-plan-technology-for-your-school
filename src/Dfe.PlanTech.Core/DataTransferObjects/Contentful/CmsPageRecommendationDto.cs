using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsPageRecommendationDto : CmsEntryDto
{
    public CmsPageRecommendationDto(PageRecommendationEntry pageRecommendationEntry)
    {
        Id= pageRecommendationEntry.Id;
        InternalName = pageRecommendationEntry.InternalName;
        InsetText = pageRecommendationEntry.InsetText?.AsDto();
    }

    public string Id { get; init; } = null!;
    public string InternalName { get; init; } = null!;
    public CmsComponentTitleDto? Title { get; init; }
    public CmsComponentInsetTextDto? InsetText { get; init; }
    public CmsComponentTextBodyDto? TextBody { get; init; }
    public IEnumerable<CmsComponentHeaderDto>? Header { get; init; }
    public IEnumerable<CmsComponentTextBodyWithMaturityDto>? TextBodyWithMaturity { get; init; }
    public List<CmsEntryDto> Content { get; init; } = [];
}
