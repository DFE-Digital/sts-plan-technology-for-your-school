using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentAccordionEntry: TransformableEntry<ComponentAccordionEntry, CmsComponentAccordionDto>
{
    public string InternalName { get; init; } = null!;
    public IEnumerable<ComponentAccordionSectionEntry> Content { get; init; } = [];
    public ComponentAccordionEntry() : base(entry => new CmsComponentAccordionDto(entry)) { }
}
