using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Web.Models.Content.Mapped.Types;

namespace Dfe.PlanTech.Web.Models.Content.Mapped;

[ExcludeFromCodeCoverage]
public class RichTextContentItem : CsContentItem
{
    public List<RichTextContentItem> Content { get; set; } = null!;
    public RichTextNodeType NodeType { get; set; } = RichTextNodeType.Unknown;
    public string Value { get; set; } = null!;
    public List<string> Tags { get; set; } = [];
}
