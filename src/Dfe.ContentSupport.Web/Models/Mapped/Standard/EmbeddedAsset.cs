using System.Diagnostics.CodeAnalysis;
using Dfe.ContentSupport.Web.Models.Mapped.Types;

namespace Dfe.ContentSupport.Web.Models.Mapped.Standard;

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
