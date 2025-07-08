using Dfe.PlanTech.Core.Content.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsCategoryDto : CmsEntryDto
    {
        public CmsCategoryDto(QuestionnaireCategoryEntry categoryEntry)
        {
            InternalName = categoryEntry.InternalName;
            Header = categoryEntry.Header.AsDto();
            Content = categoryEntry.Content.Select(c => c.AsDto()).ToList();
            Sections = categoryEntry.Sections.Select(s => s.AsDto()).ToList();
            SectionStatuses = categoryEntry.Sections.Select(s => s.Status.AsDto()).ToList();
            Completed = categoryEntry.Completed;
            RetrievalError = categoryEntry.RetrievalError;
        }

        public string InternalName { get; set; } = "";
        public CmsHeaderDto Header { get; set; } = null!;
        public List<CmsEntryDto> Content { get; set; } = null!;
        public List<CmsSectionDto> Sections { get; set; } = [];
        public IList<SqlSectionStatusDto> SectionStatuses { get; set; } = [];
        public int Completed { get; set; }
        public bool RetrievalError { get; set; }
    }
}
