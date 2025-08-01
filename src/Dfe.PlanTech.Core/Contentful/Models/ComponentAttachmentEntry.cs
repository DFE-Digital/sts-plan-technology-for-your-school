using Contentful.Core.Models;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentAttachmentEntry: TransformableEntry<ComponentAttachmentEntry, CmsComponentAttachmentDto>, IContentfulEntry
{
    public string InternalName { get; init; } = null!;
    public string? Title { get; init; }
    public Asset Asset { get; init; } = null!;

    public ComponentAttachmentEntry() : base(entry => new CmsComponentAttachmentDto(entry)) {}
}
