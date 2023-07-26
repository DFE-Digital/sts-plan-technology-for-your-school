
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using System.Text;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class OrderedListRenderer : BaseRichTextContentPartRender
{
    public OrderedListRenderer() : base(RichTextNodeType.OrderedList)
    {
    }

    public override StringBuilder AddHtml(IRichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        stringBuilder.Append("<ol>");

        rendererCollection.RenderChildren(content, stringBuilder);

        stringBuilder.Append("</ol>");

        return stringBuilder;
    }
}
