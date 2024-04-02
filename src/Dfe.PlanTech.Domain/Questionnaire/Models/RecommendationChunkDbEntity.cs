using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class RecommendationChunkDbEntity : ContentComponentDbEntity, IRecommendationChunk<AnswerDbEntity, ContentComponentDbEntity, HeaderDbEntity>
{
    public string Title { get; init; } = null!;

    public string HeaderId { get; set; } = null!;

    public HeaderDbEntity Header { get; init; } = null!;

    public List<ContentComponentDbEntity> Content { get; init; } = [];

    public List<AnswerDbEntity> Answers { get; init; } = [];

    public List<RecommendationSectionDbEntity> RecommendationSections { get; set; } = [];
}
