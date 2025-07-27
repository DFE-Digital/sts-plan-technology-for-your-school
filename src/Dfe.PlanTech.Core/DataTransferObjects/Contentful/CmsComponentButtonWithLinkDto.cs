using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsComponentButtonWithLinkDto : CmsEntryDto
    {
        public string? Id { get; set; }
        public string? InternalName { get; set; }
        public CmsComponentButtonDto Button { get; init; } = null!;
        public string Href { get; init; } = null!;

        public CmsComponentButtonWithLinkDto(ComponentButtonWithLinkEntry button)
        {
            Id = button.Id;
            InternalName = button.InternalName;
            Button = button.Button.AsDto();
            Href = button.Href;
        }

        public CmsComponentButtonWithLinkDto(CmsComponentButtonDto button, string href)
        {
            Button = button;
            Href = href;
        }
    }
}
