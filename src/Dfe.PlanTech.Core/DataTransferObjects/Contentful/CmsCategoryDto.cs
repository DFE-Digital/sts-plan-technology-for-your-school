namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsCategoryDto : CmsEntryDto
    {
        public string InternalName { get; set; } = "";
        public CmsHeaderDto Header { get; set; } = null!;
        public List<CmsEntryDto> Content { get; set; } = null!;
        public List<CmsSectionDto> Sections { get; set; } = [];
        public IList<SqlSectionStatusDto> SectionStatuses { get; set; } = [];
        public int Completed { get; set; }
        public bool RetrievalError { get; set; }
    }
}
