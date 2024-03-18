using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Domain.Persistence.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class RecommendationSectionChunkDbEntity : IDbEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public string RecommendationSectionId { get; set; } = null!;

    public RecommendationSectionDbEntity RecommendationSection { get; set; } = null!;

    public string? RecommendationChunkId { get; set; }

    public RecommendationChunkDbEntity? RecommendationChunk { get; set; }

    public bool Matches(RecommendationSectionDbEntity section, RecommendationChunkDbEntity chunk)
        => RecommendationSectionId == section.Id && RecommendationChunkId == chunk.Id;
}