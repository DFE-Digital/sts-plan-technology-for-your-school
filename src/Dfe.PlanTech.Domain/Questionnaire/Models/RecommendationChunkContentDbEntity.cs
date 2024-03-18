using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Persistence.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class RecommendationChunkContentDbEntity : IRelationshipJoinTable<RecommendationChunkContentDbEntity>, IContentComponentRelationship
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public string RecommendationChunkId { get; set; } = null!;

    public RecommendationChunkDbEntity RecommendationChunk { get; set; } = null!;

    public string? ContentComponentId { get; set; }

    public ContentComponentDbEntity? ContentComponent { get; set; }

    public bool Matches(RecommendationChunkContentDbEntity other)
    => RecommendationChunkId == other.RecommendationChunkId && ContentComponentId == other.ContentComponentId;
}