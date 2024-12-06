using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Types;

namespace Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Standard;

[ExcludeFromCodeCoverage]
public class Hyperlink : RichTextContentItem
{
    public Hyperlink()
    {
        NodeType = RichTextNodeType.Hyperlink;
    }

    public bool IsVimeo { get; set; }
    public string Uri { get; set; } = null!;
}
