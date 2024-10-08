using Dfe.ContentSupport.Web.Models.Mapped.Types;
using System.Diagnostics.CodeAnalysis;

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