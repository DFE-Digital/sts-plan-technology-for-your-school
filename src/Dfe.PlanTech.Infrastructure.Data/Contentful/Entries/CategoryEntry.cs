using Dfe.PlanTech.Core.DataTransferObjects;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Entries
{
    public class CategoryEntry : ContentfulEntry<CmsCategoryDto>
    {
        public string InternalName { get; set; } = "";
        public HeaderEntry Header { get; set; } = null!;
        public IList<ContentfulEntry<CmsEntryDto>> Content { get; set; } = null!;
        public IList<SectionEntry> Sections { get; set; } = [];
        public IList<SqlSectionStatusDto> SectionStatuses { get; set; } = [];
        public int Completed { get; set; }
        public bool RetrievalError { get; set; }

        protected override CmsCategoryDto CreateDto()
        {
            return new CmsCategoryDto
            {
                InternalName = InternalName,
                Header = Header.ToDto(),
                Content = Content.Select(c => c.ToDto()).ToList(),
                Sections = Sections.Select(s => s.ToDto()).ToList(),
                SectionStatuses = SectionStatuses.Select(ss => ss.ToDto()),
                Completed = Completed,
                RetrievalError = RetrievalError
            };
        }
    }
}
