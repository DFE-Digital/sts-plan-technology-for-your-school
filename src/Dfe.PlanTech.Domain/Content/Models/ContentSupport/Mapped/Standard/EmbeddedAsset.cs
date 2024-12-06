using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Types;

namespace Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Standard;

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
