using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsRecommendationChunkDto : CmsEntryDto
    {
        public string Id { get; set; } = null!;
        public string InternalName { get; set; } = null!;
        public string Header { get; init; } = null!;
        public List<CmsEntryDto> Content { get; init; } = [];
        public List<CmsQuestionnaireAnswerDto> Answers { get; init; } = [];
        public CmsCAndSLinkDto? CSLink { get; init; }

        public CmsRecommendationChunkDto(string header)
        {
            Header = header;
        }

        public CmsRecommendationChunkDto(RecommendationChunkEntry recommendationChunkEntry)
        {
            Id = recommendationChunkEntry.Id;
            InternalName = recommendationChunkEntry.InternalName;
            Header = recommendationChunkEntry.Header;
            Content = recommendationChunkEntry.Content.Select(BuildContentDto).ToList();
            Answers = recommendationChunkEntry.Answers.Select(a => a.AsDto()).ToList();
            CSLink = recommendationChunkEntry.CSLink?.AsDto();
        }
    }
}
