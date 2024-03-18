using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Domain.Persistence.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class RecommendationChunkAnswerDbEntity : IRelationshipJoinTable<RecommendationChunkAnswerDbEntity>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public string RecommendationChunkId { get; set; } = null!;

    public RecommendationChunkDbEntity RecommendationChunk { get; set; } = null!;

    public string? AnswerId { get; set; }

    public AnswerDbEntity? Answer { get; set; }

    public bool Matches(RecommendationChunkAnswerDbEntity other)
    => RecommendationChunkId == other.RecommendationChunkId && AnswerId == other.AnswerId;
}