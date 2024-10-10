using System.Diagnostics.CodeAnalysis;
using Dfe.ContentSupport.Web.Models.Mapped.Types;

namespace Dfe.ContentSupport.Web.Models.Mapped.Standard;

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
