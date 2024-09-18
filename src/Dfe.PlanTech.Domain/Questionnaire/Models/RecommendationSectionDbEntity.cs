using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class RecommendationSectionDbEntity : ContentComponentDbEntity, IRecommendationSection<AnswerDbEntity, ContentComponentDbEntity, RecommendationChunkDbEntity>
{
    [DontCopyValue]
    public List<AnswerDbEntity> Answers { get; init; } = [];

    [DontCopyValue]
    public List<RecommendationChunkDbEntity> Chunks { get; init; } = [];
}
