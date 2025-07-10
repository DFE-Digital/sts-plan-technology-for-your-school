using Contentful.Core.Models;
using Dfe.PlanTech.Core.Contentful.Models.Interfaces;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentAccordionEntry: TransformableEntry<ComponentAccordionEntry, CmsAccordionDto>
{
    public ComponentAccordionEntry() : base(entry => new CmsAccordionDto(entry)) {}

{
    public string Id => SystemProperties.Id;
    public string InternalName { get; init; } = null!;
    public IReadOnlyList<RichTextFieldEntry> Content { get; init; } = [];
}
