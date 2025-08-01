using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentHeaderEntry: TransformableEntry<ComponentHeaderEntry, CmsComponentHeaderDto>, IContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public string Text { get; init; } = null!;
    public HeaderTag Tag { get; init; }
    public HeaderSize Size { get; init; }

    public ComponentHeaderEntry() : base(entry => new CmsComponentHeaderDto(entry)) {}
}
