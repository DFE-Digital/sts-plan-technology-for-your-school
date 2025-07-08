using System.Text;
using System.Text.RegularExpressions;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Data.Contentful.Content.Renderers.Models.PartRenderers;

public partial class HeadingRenderer : BaseRichTextContentPartRender
{
    public HeadingRenderer() : base(RichTextNodeType.Heading)
    {
    }

    public override StringBuilder AddHtml(RichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
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
