using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

// Type name in Contentful is CsHeading
public class ComponentCsHeadingEntry: TransformableEntry<ComponentCsHeadingEntry, CmsComponentCsHeadingDto>
{
    public string InternalName { get; set; } = null!;
    public string Text { get; init; } = null!;
    public string Subtitle { get; set; } = null!;

    protected override Func<ComponentCsHeadingEntry, CmsComponentCsHeadingDto> Constructor => entry => new(entry);
}
