using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class RecommendationChunkContentDbEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public string RecommendationChunkId { get; set; } = null!;

    public RecommendationChunkDbEntity RecommendationChunk { get; set; } = null!;

    public string? ContentComponentId { get; set; }

    public ContentComponentDbEntity? ContentComponent { get; set; }

    public bool Matches(RecommendationChunkDbEntity chunk, ContentComponentDbEntity content)
        => RecommendationChunkId == chunk.Id && ContentComponentId == content.Id;
}