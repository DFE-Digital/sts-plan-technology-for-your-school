using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsComponentAccordionSectionDto : CmsEntryDto
    {
        public string Id {get;set;} = null!;
        public string InternalName {get;set;} = null!;
        public string? Title {get;set;} = null!;
        public string SummaryLine {get;set;} = null!;
        public CmsRichTextContentDto RichText { get; set; } = null!;

        public CmsComponentAccordionSectionDto(ComponentAccordionSectionEntry accordionSectionEntry)
        {
            Id = accordionSectionEntry.Id;
            InternalName = accordionSectionEntry.InternalName;
            Title = accordionSectionEntry.Title;
            RichText = accordionSectionEntry.RichText.AsDto();
        }
    }
}
