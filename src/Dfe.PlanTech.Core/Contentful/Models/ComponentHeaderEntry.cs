using Contentful.Core.Models;
using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentHeaderEntry: TransformableEntry<ComponentHeaderEntry, CmsComponentHeaderDto>
{
    public string Id => SystemProperties.Id;
    public string InternalName { get; set; } = null!;
    public string Text { get; init; } = null!;
    public HeaderTag Tag { get; init; }
    public HeaderSize Size { get; init; }

    public ComponentHeaderEntry() : base(entry => new CmsComponentHeaderDto(entry)) {}
}
