using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class ComponentAccordionSectionEntry : Entry<ContentComponent>
{
    public string InternalName { get; init; } = null!;
    public string? Title { get; init; }
    public string SummaryLine { get; init; } = null!;
    public RichTextContent RichText { get; init; } = null!;
}
