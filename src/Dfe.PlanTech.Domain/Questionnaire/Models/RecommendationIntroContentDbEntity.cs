using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Persistence.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class RecommendationIntroContentDbEntity : IRelationshipJoinTable<RecommendationIntroContentDbEntity>, IContentComponentRelationship
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public string RecommendationIntroId { get; set; } = null!;

    public RecommendationIntroDbEntity RecommendationIntro { get; set; } = null!;

    public string? ContentComponentId { get; set; }

    public ContentComponentDbEntity? ContentComponent { get; set; }

    public bool Matches(RecommendationIntroContentDbEntity other)
        => RecommendationIntroId == other.RecommendationIntroId && ContentComponentId == other.ContentComponentId;
}