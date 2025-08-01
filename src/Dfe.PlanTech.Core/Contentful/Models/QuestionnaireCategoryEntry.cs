using Contentful.Core.Models;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class QuestionnaireCategoryEntry : TransformableEntry<QuestionnaireCategoryEntry, CmsCategoryDto>, IContentfulEntry
{
    public string InternalName { get; set; } = "";
    public ComponentHeaderEntry Header { get; set; } = null!;
    public List<Entry<ContentComponent>> Content { get; set; } = null!;
    public List<QuestionnaireSectionEntry> Sections { get; set; } = [];

    public QuestionnaireCategoryEntry(IEnumerable<SqlSectionStatusDto> sectionStatuses) : base(entry => new CmsCategoryDto(entry, sectionStatuses)) { }
}
