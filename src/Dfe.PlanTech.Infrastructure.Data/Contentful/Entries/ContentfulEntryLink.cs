using Dfe.PlanTech.Core.DataTransferObjects;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Entries
{
    public class ContentfulEntryLink
    {
        public string? Id { get; set; }
        public string? LinkType { get; set; }
        public string? Type { get; set; }

        public CmsEntryLinkDto ToDto()
        {
            return new CmsEntryLinkDto
            {
                Id = Id,
                LinkType = LinkType,
                Type = Type
            };
        }
    }
}
