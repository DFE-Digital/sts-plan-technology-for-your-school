using Contentful.Core.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

// Type name in Contentful is CsHeading
public class ComponentCsHeadingEntry: TransformableEntry<ComponentCsHeadingEntry, CmsComponentCsHeadingDto>
{
    public string Id => SystemProperties.Id;
    public string InternalName { get; set; } = null!;
    public string Text { get; init; } = null!;
    public string Subtitle { get; set; } = null!;

    public ComponentCsHeadingEntry() : base(entry => new CmsComponentCsHeadingDto(entry)) {}
}
