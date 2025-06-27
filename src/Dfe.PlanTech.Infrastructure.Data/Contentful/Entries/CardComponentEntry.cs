using Contentful.Core.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Entries
{
    public class CardComponentEntry : ContentfulEntry<CmsCardComponentDto>
    {
        public string InternalName { get; init; } = null!;
        public string? Title { get; init; } = null!;
        public string? Meta { get; init; } = null!;
        public Asset? Image { get; init; } = null!;
        public string? ImageAlt { get; init; } = null!;
        public string? Uri { get; init; } = null!;

        protected override CmsCardComponentDto CreateDto()
        {
            return new CmsCardComponentDto
            {
                InternalName = InternalName,
                Title = Title,
                Meta = Meta,
                Image = Image,
                ImageAlt = ImageAlt,
                Uri = Uri
            };
        }
    }
}
