using Dfe.ContentSupport.Web.Models.Mapped.Types;
using System.Diagnostics.CodeAnalysis;

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