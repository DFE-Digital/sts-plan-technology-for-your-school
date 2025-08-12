using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class QuestionnaireCategoryEntry : TransformableEntry<QuestionnaireCategoryEntry, CmsQuestionnaireCategoryDto>
{
    public string InternalName { get; set; } = "";
    public ComponentHeaderEntry Header { get; set; } = null!;
    public List<ContentfulEntry>? Content { get; set; }
    public List<QuestionnaireSectionEntry> Sections { get; set; } = [];
    public PageEntry? LandingPage { get; set; }

    protected override Func<QuestionnaireCategoryEntry, CmsQuestionnaireCategoryDto> Constructor => entry => new(entry);
}
