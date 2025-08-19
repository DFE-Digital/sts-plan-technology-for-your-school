using Dfe.PlanTech.Core.Contentful.Enums;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentHeaderEntry: ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public string Text { get; init; } = null!;
    public HeaderTag Tag { get; init; }
    public HeaderSize Size { get; init; }
}
