using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Domain.Persistence.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class RecommendationSectionChunkDbEntity : IRelationshipJoinTable<RecommendationSectionChunkDbEntity>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public string RecommendationSectionId { get; set; } = null!;

    public RecommendationSectionDbEntity RecommendationSection { get; set; } = null!;

    public string? RecommendationChunkId { get; set; }

    public RecommendationChunkDbEntity? RecommendationChunk { get; set; }

    public bool Matches(RecommendationSectionChunkDbEntity other)
        => RecommendationChunkId == other.RecommendationChunkId && RecommendationSectionId == other.RecommendationSectionId;
}