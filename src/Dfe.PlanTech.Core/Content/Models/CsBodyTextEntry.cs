using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class CsBodyTextEntry : Entry<ContentComponent>
{
    public string InternalName { get; set; } = null!;
    public RichTextContent RichText { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Subtitle { get; set; } = null!;
}
