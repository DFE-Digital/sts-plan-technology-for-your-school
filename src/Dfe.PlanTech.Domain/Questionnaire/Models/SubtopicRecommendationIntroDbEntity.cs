using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Domain.Persistence.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class SubtopicRecommendationIntroDbEntity : IRelationshipJoinTable<SubtopicRecommendationIntroDbEntity>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    public string SubtopicRecommendationId { get; set; } = null!;

    public SubtopicRecommendationDbEntity SubtopicRecommendation { get; set; } = null!;
    public string? RecommendationIntroId { get; set; }

    public RecommendationIntroDbEntity? RecommendationIntro { get; set; }

    public bool Matches(SubtopicRecommendationIntroDbEntity other)
        => SubtopicRecommendationId == other.SubtopicRecommendationId && RecommendationIntroId == other.RecommendationIntroId;
}