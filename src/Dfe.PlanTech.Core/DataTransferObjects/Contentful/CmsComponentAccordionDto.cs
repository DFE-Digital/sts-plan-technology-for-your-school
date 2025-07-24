using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsComponentAccordionDto : CmsEntryDto
    {
        public string Id { get; set; } = null!;
        public string InternalName { get; init; } = null!;
        public List<CmsComponentAccordionSectionDto> Content { get; init; } = [];

        public CmsComponentAccordionDto(ComponentAccordionEntry accordionEntry)
        {
            Id = accordionEntry.Id;
            InternalName = accordionEntry.InternalName;
            Content = accordionEntry.Content.Select(c => c.AsDto()).ToList();
        }
    }
}
