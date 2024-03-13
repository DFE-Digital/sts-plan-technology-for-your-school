using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class RecommendationIntroContentDbEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    
    public string RecommendationIntroId { get; set; } = null!;
    
    public RecommendationIntroDbEntity RecommendationIntro { get; set; } = null!;
    
    public string? ContentComponentId { get; set; }
    
    public ContentComponentDbEntity? ContentComponent { get; set; }
    
}