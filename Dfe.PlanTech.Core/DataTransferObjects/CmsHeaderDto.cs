using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Core.DataTransferObjects
{
    public class CmsHeaderDto
    {
        public string Text { get; init; } = null!;

        public HeaderTag Tag { get; init; }

        public HeaderSize Size { get; init; }
    }
}
