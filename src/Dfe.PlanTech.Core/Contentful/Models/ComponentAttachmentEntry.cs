using Contentful.Core.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentAttachmentEntry: TransformableEntry<ComponentAttachmentEntry, CmsComponentAttachmentDto>
{
    public string InternalName { get; init; } = null!;
    public string? Title { get; init; }
    public Asset Asset { get; init; } = null!;

    protected override Func<ComponentAttachmentEntry, CmsComponentAttachmentDto> Constructor => entry => new(entry);
}
