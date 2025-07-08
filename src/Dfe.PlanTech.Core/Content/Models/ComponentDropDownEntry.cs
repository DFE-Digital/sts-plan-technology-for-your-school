using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class ComponentDropDownEntry : Entry<ContentComponent>
{
    public string InternalName { get; set; } = null!;
    public string Title { get; set; } = null!;
    public RichTextContent? Content { get; set; } = null!;
}
