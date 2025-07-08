using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class RecommendationChunkEntry : Entry<ContentComponent>
{
    public string InternalName { get; set; } = null!;
    public string Header { get; init; } = null!;
    public List<ContentComponent> Content { get; init; } = [];
    public List<QuestionnaireAnswerEntry> Answers { get; init; } = [];
    public CAndSLinkEntry? CSLink { get; init; }
}
