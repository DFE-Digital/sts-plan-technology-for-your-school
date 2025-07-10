using Contentful.Core.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentButtonWithEntryReferenceEntry: TransformableEntry<ComponentButtonWithEntryReferenceEntry, CmsComponentButtonWithEntryReferenceDto>
{
    public string Id => SystemProperties.Id;
    public string InternalName { get; set; } = null!;
    public ComponentButtonEntry Button { get; init; } = null!;
    public ContentComponent LinkToEntry { get; init; } = null!;

    public ComponentButtonWithEntryReferenceEntry() : base(entry => new CmsComponentButtonWithEntryReferenceDto(entry)) { }
}
