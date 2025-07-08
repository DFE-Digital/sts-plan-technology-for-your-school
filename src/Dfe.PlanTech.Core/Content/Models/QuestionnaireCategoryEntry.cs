using Contentful.Core.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Content.Models;

public class QuestionnaireCategoryEntry : Entry<ContentComponent>
{
    public string InternalName { get; set; } = "";
    public ComponentHeaderEntry Header { get; set; } = null!;
    public List<ContentComponent> Content { get; set; } = null!;
    public List<QuestionnaireSectionEntry> Sections { get; set; } = [];

    public CmsCategoryDto AsDto => new(this);
}
