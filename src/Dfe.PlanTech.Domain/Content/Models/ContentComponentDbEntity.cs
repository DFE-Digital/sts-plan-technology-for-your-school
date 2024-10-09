using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Content.Models;

public class ContentComponentDbEntity : IContentComponentDbEntity
{
    [DontCopyValue]
    public string Id { get; set; } = null!;

    public bool Published { get; set; }

    public bool Archived { get; set; }

    public bool Deleted { get; set; }

    [DontCopyValue]
    public long? Order { get; set; }

    public List<PageDbEntity> BeforeTitleContentPages { get; set; } = [];


    public List<PageDbEntity> ContentPages { get; set; } = [];
    
    public List<RecommendationChunkDbEntity> RecommendationChunk { get; set; } = [];

    public List<RecommendationChunkContentDbEntity> RecommendationChunkContentJoins { get; set; } = [];


    public List<RecommendationIntroDbEntity> RecommendationIntro { get; set; } = [];

    public List<RecommendationIntroContentDbEntity> RecommendationIntroContentJoins { get; set; } = [];

}
