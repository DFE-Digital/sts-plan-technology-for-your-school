using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsHeaderDto : CmsEntryDto
    {
        public string Text { get; set; } = null!;

        public HeaderTag Tag { get; set; }

        public HeaderSize Size { get; set; }
    }
}
