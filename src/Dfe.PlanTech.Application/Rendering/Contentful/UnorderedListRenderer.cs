using System.Text;
using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Contentful.Rendering;

namespace Dfe.PlanTech.Application.Rendering.Contentful;

public class UnorderedListRenderer : BaseRichTextContentPartRenderer
{
    public UnorderedListRenderer()
        : base(RichTextNodeType.UnorderedList) { }

    public override StringBuilder AddHtml(
        RichTextContentField content,
        IRichTextContentPartRendererCollection rendererCollection,
        StringBuilder stringBuilder
    )
    {
        stringBuilder.Append("<ul>");

        rendererCollection.RenderChildren(content, stringBuilder);

        stringBuilder.Append("</ul>");

        return stringBuilder;
    }
}
