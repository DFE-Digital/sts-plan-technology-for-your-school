using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsComponentButtonWithLinkDto : CmsEntryDto
    {
        public string Id { get; set; } = null!;
        public string InternalName { get; set; } = null!;
        public CmsComponentButtonDto Button { get; init; } = null!;
        public string Href { get; init; } = null!;

        public CmsComponentButtonWithLinkDto(ComponentButtonWithLinkEntry button)
        {
            Id = button.Id;
            InternalName = button.InternalName;
            Button = button.Button.AsDto();
            Href = button.Href;
        }
    }
}
