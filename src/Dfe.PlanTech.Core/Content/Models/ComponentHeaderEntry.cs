using Contentful.Core.Models;
using Dfe.PlanTech.Domain.Content.Enums;

namespace Dfe.PlanTech.Core.Content.Models;

public class ComponentHeaderEntry : Entry<ContentComponent>
{
    public string InternalName { get; set; } = null!;
    public string Text { get; init; } = null!;
    public HeaderTag Tag { get; init; }
    public HeaderSize Size { get; init; }
}
