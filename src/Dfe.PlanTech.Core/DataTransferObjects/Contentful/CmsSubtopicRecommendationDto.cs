using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsSubtopicRecommendationDto : CmsEntryDto
{
    public string Id { get; set; } = null!;
    public string InternalName { get; set; } = null!;
    public List<CmsRecommendationIntroDto> Intros { get; set; } = [];
    public CmsRecommendationSectionDto Section { get; set; } = null!;
    public CmsQuestionnaireSectionDto Subtopic { get; set; } = null!;

    public CmsSubtopicRecommendationDto(SubtopicRecommendationEntry subtopicRecommendationEntry)
    {
        Id = subtopicRecommendationEntry.Id;
        InternalName = subtopicRecommendationEntry.InternalName;
        Intros = subtopicRecommendationEntry.Intros.Select(i => i.AsDto()).ToList();
        Section = subtopicRecommendationEntry.Section.AsDto();
        Subtopic = subtopicRecommendationEntry.Subtopic.AsDto();
    }

    public CmsRecommendationIntroDto? GetRecommendationByMaturity(string? maturity) =>
        Intros.FirstOrDefault(intro => !string.IsNullOrWhiteSpace(intro.Maturity) && intro.Maturity.Equals(maturity));
}
