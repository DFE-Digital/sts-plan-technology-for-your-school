using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Extensions;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsRecommendationChunkDto : CmsEntryDto, IHeaderWithContent
{
    public string Id { get; set; } = null!;
    public string InternalName { get; set; } = null!;
    public string HeaderText { get; init; } = null!;
    public List<CmsEntryDto> Content { get; init; } = [];
    public List<CmsQuestionnaireAnswerDto> Answers { get; init; } = [];
    public CmsCAndSLinkDto? CSLink { get; init; }

    public string LinkText => HeaderText;
    public string SlugifiedLinkText => LinkText.Slugify();

    public CmsRecommendationChunkDto(string header)
    {
        HeaderText = header;
    }

    public CmsRecommendationChunkDto(RecommendationChunkEntry recommendationChunkEntry)
    {
        Id = recommendationChunkEntry.Id;
        InternalName = recommendationChunkEntry.InternalName;
        HeaderText = recommendationChunkEntry.Header;
        Content = recommendationChunkEntry.Content.Select(BuildContentDto).ToList();
        Answers = recommendationChunkEntry.Answers.Select(a => a.AsDto()).ToList();
        CSLink = recommendationChunkEntry.CSLink?.AsDto();
    }
}
