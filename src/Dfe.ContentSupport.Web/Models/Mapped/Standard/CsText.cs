using System.Diagnostics.CodeAnalysis;
using Dfe.ContentSupport.Web.Models.Mapped.Types;

namespace Dfe.ContentSupport.Web.Models.Mapped.Standard;

[ExcludeFromCodeCoverage]
public class CsText : RichTextContentItem
{
    public CsText()
    {
        NodeType = RichTextNodeType.Text;
    }

    public bool IsBold { get; set; }
}
