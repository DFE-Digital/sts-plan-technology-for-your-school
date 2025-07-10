using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsComponentHeaderDto : CmsEntryDto
    {
        public string Id { get; set; } = null!;
        public string InternalName { get; set; } = null!;
        public string Text { get; init; } = null!;
        public HeaderTag Tag { get; init; }
        public HeaderSize Size { get; init; }

        public CmsComponentHeaderDto(ComponentHeaderEntry headerEntry)
        {
            Id = headerEntry.Id;
            InternalName = headerEntry.InternalName;
            Text = headerEntry.Text;
            Tag = headerEntry.Tag;
            Size = headerEntry.Size;
        }
    }
}
