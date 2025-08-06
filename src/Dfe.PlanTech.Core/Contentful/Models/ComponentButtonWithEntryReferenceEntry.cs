using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentButtonWithEntryReferenceEntry: TransformableEntry<ComponentButtonWithEntryReferenceEntry, CmsComponentButtonWithEntryReferenceDto>
{
    public string InternalName { get; set; } = null!;
    public ComponentButtonEntry Button { get; init; } = null!;
    public ContentfulEntry LinkToEntry { get; init; } = null!;

    protected override Func<ComponentButtonWithEntryReferenceEntry, CmsComponentButtonWithEntryReferenceDto> Constructor => entry => new(entry);
}
