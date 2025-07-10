using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsCategoryDto : CmsEntryDto
    {
        public CmsCategoryDto(QuestionnaireCategoryEntry categoryEntry)
        {
            InternalName = categoryEntry.InternalName;
            Header = categoryEntry.Header.AsDto();
            Content = categoryEntry.Content.Select(BuildContentDto).ToList();
            Sections = categoryEntry.Sections.Select(s => s.AsDto()).ToList();
        }

        public string InternalName { get; set; } = "";
        public CmsComponentHeaderDto Header { get; set; } = null!;
        public List<CmsEntryDto> Content { get; set; } = null!;
        public List<CmsQuestionnaireSectionDto> Sections { get; set; } = [];
    }
}
