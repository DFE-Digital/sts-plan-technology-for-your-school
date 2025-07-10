using Contentful.Core.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class QuestionnaireCategoryEntry : TransformableEntry<QuestionnaireCategoryEntry, CmsCategoryDto>
{
    public string Id => SystemProperties.Id;
    public string InternalName { get; set; } = "";
    public ComponentHeaderEntry Header { get; set; } = null!;
    public List<ContentComponent> Content { get; set; } = null!;
    public List<QuestionnaireSectionEntry> Sections { get; set; } = [];

    public QuestionnaireCategoryEntry() : base(entry => new CmsCategoryDto(entry)) { }
}
