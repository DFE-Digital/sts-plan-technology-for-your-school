using Dfe.PlanTech.Core.DataTransferObjects;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Entries
{
    public class CategoryEntry : ContentfulEntry
    {
        public string InternalName { get; set; } = "";
        public HeaderEntry Header { get; set; } = null!;
        public List<ContentfulEntry> Content { get; set; } = null!;
        public List<SectionEntry> Sections { get; set; } = [];
        public IList<SqlSectionStatusDto> SectionStatuses { get; set; } = [];
        public int Completed { get; set; }
        public bool RetrievalError { get; set; }
    }
}
}
