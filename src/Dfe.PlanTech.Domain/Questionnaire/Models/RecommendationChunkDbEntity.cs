using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class RecommendationChunkDbEntity : ContentComponentDbEntity, IRecommendationChunk<AnswerDbEntity, ContentComponentDbEntity, HeaderDbEntity>
{
    public string HeaderId { get; set; } = null!;

    public HeaderDbEntity Header { get; init; } = null!;

    [NotMapped]
    public List<ContentComponentDbEntity> Content { get; init; } = [];

    [NotMapped]
    public List<RecommendationChunkContentDbEntity> ChunkContentJoins { get; init; } = [];

    [NotMapped]
    public List<AnswerDbEntity> Answers { get; init; } = [];

    [DontCopyValue]
    public List<RecommendationSectionDbEntity> RecommendationSections { get; set; } = [];
}
