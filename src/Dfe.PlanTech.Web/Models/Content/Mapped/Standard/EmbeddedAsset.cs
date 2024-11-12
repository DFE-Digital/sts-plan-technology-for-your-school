using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Web.Models.Content.Mapped.Types;

namespace Dfe.PlanTech.Web.Models.Content.Mapped.Standard;

[ExcludeFromCodeCoverage]
public class EmbeddedAsset : RichTextContentItem
{
    public EmbeddedAsset()
    {
        NodeType = RichTextNodeType.EmbeddedAsset;
    }

    public AssetContentType AssetContentType { get; set; } = AssetContentType.Unknown;

    public string Description { get; set; } = null!;
    public string Uri { get; set; } = null!;
}
