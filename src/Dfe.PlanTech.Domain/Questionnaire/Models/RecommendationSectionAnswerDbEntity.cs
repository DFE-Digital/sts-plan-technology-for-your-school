using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Domain.Persistence.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class RecommendationSectionAnswerDbEntity : IRelationshipJoinTable<RecommendationSectionAnswerDbEntity>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public string RecommendationSectionId { get; set; } = null!;

    public RecommendationSectionDbEntity RecommendationSection { get; set; } = null!;

    public string? AnswerId { get; set; }

    public AnswerDbEntity? Answer { get; set; }

    public bool Matches(RecommendationSectionAnswerDbEntity other)
        => RecommendationSectionId == other.RecommendationSectionId && AnswerId == other.AnswerId;
}