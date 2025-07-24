using System.Text;
using System.Text.RegularExpressions;
using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Application.Rendering;

public partial class HeadingRenderer : BaseRichTextContentPartRenderer
{
    public HeadingRenderer() : base(RichTextNodeType.Heading)
    {
    }

    public override StringBuilder AddHtml(CmsRichTextContentDto content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        var tag = GetHeaderTag().Replace(content.NodeType, "$1$2");

        stringBuilder.Append($"<{tag}>");
        rendererCollection.RenderChildren(content, stringBuilder);
        stringBuilder.Append($"</{tag}>");

        return stringBuilder;
    }

    [GeneratedRegex("(h)eading-(\\d)")]
    private static partial Regex GetHeaderTag();
}
