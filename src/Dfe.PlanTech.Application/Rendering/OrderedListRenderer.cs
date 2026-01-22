using System.Text;
using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.Rendering;

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
