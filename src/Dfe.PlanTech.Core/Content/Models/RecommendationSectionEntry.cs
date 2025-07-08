using System.ComponentModel.DataAnnotations.Schema;
using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class RecommendationSectionEntry : Entry<ContentComponent>
{
    public string InternalName { get; set; } = null!;
    [NotMapped]
    public List<QuestionnaireAnswerEntry> Answers { get; init; } = [];
    [NotMapped]
    public IEnumerable<RecommendationChunkEntry> Chunks { get; init; } = [];
}
