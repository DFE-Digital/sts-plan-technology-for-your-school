using System.Text;
using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Contentful.Rendering;

namespace Dfe.PlanTech.Application.Rendering.Contentful;

public class OrderedListRenderer : BaseRichTextContentPartRenderer
{
    public OrderedListRenderer()
        : base(RichTextNodeType.OrderedList) { }

    public override StringBuilder AddHtml(
        RichTextContentField content,
        IRichTextContentPartRendererCollection rendererCollection,
        StringBuilder stringBuilder
    )
    {
        stringBuilder.Append("<ol>");

        rendererCollection.RenderChildren(content, stringBuilder);

        stringBuilder.Append("</ol>");

        return stringBuilder;
    }
}
