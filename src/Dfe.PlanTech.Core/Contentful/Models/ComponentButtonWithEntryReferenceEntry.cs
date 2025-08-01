using Contentful.Core.Models;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentButtonWithEntryReferenceEntry: TransformableEntry<ComponentButtonWithEntryReferenceEntry, CmsComponentButtonWithEntryReferenceDto>, IContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public ComponentButtonEntry Button { get; init; } = null!;
    public Entry<ContentComponent> LinkToEntry { get; init; } = null!;

    public ComponentButtonWithEntryReferenceEntry() : base(entry => new CmsComponentButtonWithEntryReferenceDto(entry)) { }
}
