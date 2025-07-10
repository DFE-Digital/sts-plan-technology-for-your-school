using Contentful.Core.Models;
using Dfe.PlanTech.Core.Contentful.Models.Interfaces;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentAccordionSectionEntry: TransformableEntry<ComponentAccordionSectionEntry, CmsDto>
{
    public ComponentAccordionSectionEntry() : base(entry => new CmsDto(entry)) {}

{
    public string Id => SystemProperties.Id;
    public string InternalName { get; init; } = null!;
    public string? Title { get; init; }
    public string SummaryLine { get; init; } = null!;
    public RichTextContent RichText { get; init; } = null!;
}
