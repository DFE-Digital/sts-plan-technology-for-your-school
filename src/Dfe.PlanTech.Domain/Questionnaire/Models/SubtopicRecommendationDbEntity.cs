using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class SubtopicRecommendationDbEntity : ContentComponentDbEntity, ISubTopicRecommendation<AnswerDbEntity, ContentComponentDbEntity, HeaderDbEntity, RecommendationChunkDbEntity, RecommendationIntroDbEntity, RecommendationSectionDbEntity, SectionDbEntity>
{
    [DontCopyValue]
    public List<RecommendationIntroDbEntity> Intros { get; init; } = [];

    public string SectionId { get; set; } = null!;

    public RecommendationSectionDbEntity Section { get; init; } = null!;

    public string SubtopicId { get; set; } = null!;

    public SectionDbEntity Subtopic { get; init; } = null!;
}
