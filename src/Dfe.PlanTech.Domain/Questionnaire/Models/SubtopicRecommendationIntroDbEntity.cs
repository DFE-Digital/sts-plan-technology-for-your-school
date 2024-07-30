using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class SubtopicRecommendationIntroDbEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    public string SubtopicRecommendationId { get; set; } = null!;

    public SubtopicRecommendationDbEntity SubtopicRecommendation { get; set; } = null!;
    public string? RecommendationIntroId { get; set; }

    public RecommendationIntroDbEntity? RecommendationIntro { get; set; }
}
