using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class QuestionnaireSectionEntry : TransformableEntry<QuestionnaireSectionEntry, CmsQuestionnaireSectionDto>
{
    public string InternalName { get; set; } = null!;
    public string Name { get; init; } = null!;
    public PageEntry InterstitialPage { get; set; } = null!;
    public IEnumerable<QuestionnaireQuestionEntry> Questions { get; init; } = [];

    protected override Func<QuestionnaireSectionEntry, CmsQuestionnaireSectionDto> Constructor => entry => new(entry);
}
