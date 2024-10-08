using Dfe.ContentSupport.Web.Models.Mapped.Custom;
using Dfe.ContentSupport.Web.Models.Mapped.Types;
using System.Diagnostics.CodeAnalysis;

namespace Dfe.ContentSupport.Web.Models.Mapped.Standard;

[ExcludeFromCodeCoverage]
public class EmbeddedEntry : RichTextContentItem
{
    public EmbeddedEntry()
    {
        NodeType = RichTextNodeType.EmbeddedEntry;
    }

    public string JumpIdentifier { get; set; } = null!;
    public RichTextContentItem? RichText { get; set; }
    public CustomComponent? CustomComponent { get; set; }
}