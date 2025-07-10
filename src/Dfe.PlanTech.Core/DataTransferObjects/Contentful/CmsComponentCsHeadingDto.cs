using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsComponentCsHeadingDto : CmsEntryDto
    {
        public string Id { get; set; } = null!;
        public string InternalName { get; set; } = null!;
        public string Text { get; init; } = null!;
        public string Subtitle { get; set; } = null!;

        public CmsComponentCsHeadingDto(ComponentCsHeadingEntry csHeadingEntry)
        {
            Id = csHeadingEntry.Id;
            InternalName = csHeadingEntry.InternalName;
            Text = csHeadingEntry.Text;
            Subtitle = csHeadingEntry.Subtitle;
        }
    }
}
