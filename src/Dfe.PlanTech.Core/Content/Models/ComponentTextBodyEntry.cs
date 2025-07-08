using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class ComponentTextBodyEntry : Entry<ContentComponent>
{
    public string InternalName { get; set; } = null!;
    public RichTextContent RichText { get; init; } = null!;
}
