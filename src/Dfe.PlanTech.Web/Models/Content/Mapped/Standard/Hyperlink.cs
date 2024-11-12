using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Web.Models.Content.Mapped.Types;

namespace Dfe.PlanTech.Web.Models.Content.Mapped.Standard;

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
