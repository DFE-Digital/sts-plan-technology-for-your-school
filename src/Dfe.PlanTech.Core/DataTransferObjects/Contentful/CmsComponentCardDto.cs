using Contentful.Core.Models;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsComponentCardDto : CmsEntryDto
    {
        public string Id { get; set; } = null!;
        public string InternalName { get; init; } = null!;
        public string? Title { get; init; } = null!;
        public string? Meta { get; init; } = null!;
        public Asset? Image { get; init; } = null!;
        public string? ImageAlt { get; init; } = null!;
        public string? Uri { get; init; } = null!;

        public CmsComponentCardDto(ComponentCardEntry cardEntry)
        {
            Id = cardEntry.Id;
            InternalName = cardEntry.InternalName;
            Title = cardEntry.Title;
            Description = cardEntry.Description;
            Meta = cardEntry.Meta;
            Image = cardEntry.Image;
            ImageAlt = cardEntry.ImageAlt;
            Uri = cardEntry.Uri;
        }
    }
}
