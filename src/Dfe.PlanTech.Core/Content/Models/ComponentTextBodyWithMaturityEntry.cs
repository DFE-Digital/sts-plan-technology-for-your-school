using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class ComponentTextBodyWithMaturityEntry : Entry<ContentComponent>
{
    public string InternalName { get; set; } = null!;
    public RichTextContent TextBody { get; init; } = null!;
    public string Maturity { get; set; } = null!;
}
