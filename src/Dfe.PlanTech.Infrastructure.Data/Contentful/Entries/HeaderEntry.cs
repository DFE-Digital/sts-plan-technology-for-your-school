using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Entries
{
    public class HeaderEntry : ContentfulEntry<CmsHeaderDto>
    {
        public string Text { get; init; } = null!;

        public HeaderTag Tag { get; init; }

        public HeaderSize Size { get; init; }

        protected override CmsHeaderDto CreateDto()
        {
            return new CmsHeaderDto
            {
                Text = Text,
                Tag = Tag,
                Size = Size
            };
        }
    }
}
