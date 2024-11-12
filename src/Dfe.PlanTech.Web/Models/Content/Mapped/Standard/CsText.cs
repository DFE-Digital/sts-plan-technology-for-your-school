using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Web.Models.Content.Mapped.Types;

namespace Dfe.PlanTech.Web.Models.Content.Mapped.Standard;

[ExcludeFromCodeCoverage]
public class CsText : RichTextContentItem
{
    public CsText()
    {
        NodeType = RichTextNodeType.Text;
    }

    public bool IsBold { get; set; }
}
