using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class ComponentAccordionEntry : Entry<ContentComponent>
{
    public string InternalName { get; init; } = null!;
    public IReadOnlyList<RichTextContentData> Content { get; init; } = [];
}
