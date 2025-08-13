using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsPageRecommendationDto : CmsEntryDto
{
    public string Id { get; init; } = null!;
    public string InternalName { get; init; } = null!;
    public CmsComponentTitleDto? Title { get; init; }
    public CmsComponentInsetTextDto? InsetText { get; init; }
    public CmsComponentTextBodyDto? TextBody { get; init; }
    public IEnumerable<CmsComponentHeaderDto>? Headers { get; init; }
    public IEnumerable<CmsComponentTextBodyWithMaturityDto>? TextBodyWithMaturity { get; init; }
    public List<CmsEntryDto> Content { get; init; } = [];

    public CmsPageRecommendationDto(PageRecommendationEntry pageRecommendationEntry)
    {
        Id = pageRecommendationEntry.Id;
        InternalName = pageRecommendationEntry.InternalName;
        Title = pageRecommendationEntry.Title?.AsDto();
        InsetText = pageRecommendationEntry.InsetText?.AsDto();
        TextBody = pageRecommendationEntry.TextBody?.AsDto();
        Headers = pageRecommendationEntry.Header?.Select(h => h.AsDto()).ToList();
        TextBodyWithMaturity = pageRecommendationEntry.TextBodyWithMaturity?.Select(tbwm => tbwm.AsDto()).ToList();
        Content = pageRecommendationEntry.Content.Select(BuildContentDto).ToList();
    }
}
