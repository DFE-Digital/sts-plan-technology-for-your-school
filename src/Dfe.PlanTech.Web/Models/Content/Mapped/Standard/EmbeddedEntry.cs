using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Web.Models.Content.Mapped.Custom;
using Dfe.PlanTech.Web.Models.Content.Mapped.Types;

namespace Dfe.PlanTech.Web.Models.Content.Mapped.Standard;

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
