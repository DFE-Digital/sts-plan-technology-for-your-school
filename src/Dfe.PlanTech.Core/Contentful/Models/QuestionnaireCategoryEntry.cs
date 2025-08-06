using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class QuestionnaireCategoryEntry : TransformableEntry<QuestionnaireCategoryEntry, CmsCategoryDto>
{
    public string InternalName { get; set; } = "";
    public ComponentHeaderEntry Header { get; set; } = null!;
    public List<ContentfulEntry>? Content { get; set; }
    public List<QuestionnaireSectionEntry> Sections { get; set; } = [];

    protected override Func<QuestionnaireCategoryEntry, CmsCategoryDto> Constructor => entry => new(entry);
}
