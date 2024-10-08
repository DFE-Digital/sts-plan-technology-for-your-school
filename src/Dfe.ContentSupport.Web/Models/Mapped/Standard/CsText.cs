using Dfe.ContentSupport.Web.Models.Mapped.Types;
using System.Diagnostics.CodeAnalysis;

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