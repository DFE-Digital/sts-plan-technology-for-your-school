using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Types;

namespace Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Standard;

[ExcludeFromCodeCoverage]
public class CsText : RichTextContentItem
{
    public CsText()
    {
        NodeType = RichTextNodeType.Text;
    }

    public bool IsBold { get; set; }
}
