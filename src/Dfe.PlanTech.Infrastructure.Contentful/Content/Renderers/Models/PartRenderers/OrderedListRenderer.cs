
using System.Text;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class OrderedListRenderer : BaseRichTextContentPartRender
{
    public OrderedListRenderer() : base(RichTextNodeType.OrderedList)
    {
    }

    public override StringBuilder AddHtml(RichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        stringBuilder.Append("<ol>");

        rendererCollection.RenderChildren(content, stringBuilder);

        stringBuilder.Append("</ol>");

        return stringBuilder;
    }
}
