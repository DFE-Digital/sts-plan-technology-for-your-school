using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class QuestionnaireSectionEntry : Entry<ContentComponent>
{
    public string InternalName { get; set; } = null!;
    public string Name { get; init; } = null!;
    public PageEntry InterstitialPage { get; set; } = null!;
    public IEnumerable<QuestionnaireQuestionEntry> Questions { get; init; } = [];
}
