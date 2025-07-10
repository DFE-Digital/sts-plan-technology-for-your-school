using Contentful.Core.Models;
using Dfe.PlanTech.Core.Contentful.Models.Interfaces;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentAttachmentEntry: TransformableEntry<ComponentAttachmentEntry, CmsDto>
{
    public ComponentAttachmentEntry() : base(entry => new CmsDto(entry)) {}

{
    public string Id => SystemProperties.Id;
    public string InternalName { get; init; } = null!;
    public string? Title { get; init; }
    public Asset Asset { get; init; } = null!;
}
