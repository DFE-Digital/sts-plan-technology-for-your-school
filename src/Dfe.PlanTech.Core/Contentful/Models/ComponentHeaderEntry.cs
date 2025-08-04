using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentHeaderEntry: TransformableEntry<ComponentHeaderEntry, CmsComponentHeaderDto>
{
    public string InternalName { get; set; } = null!;
    public string Text { get; init; } = null!;
    public HeaderTag Tag { get; init; }
    public HeaderSize Size { get; init; }

    protected override Func<ComponentHeaderEntry, CmsComponentHeaderDto> Constructor => entry => new(entry);
}
