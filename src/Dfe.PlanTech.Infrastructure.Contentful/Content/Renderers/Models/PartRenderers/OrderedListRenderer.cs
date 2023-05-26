
using System.Text;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Interfaces;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class OrderedListRenderer : RichTextContentRender
{
    public OrderedListRenderer() : base(RichTextNodeType.OrderedList)
    {
    }

    public override StringBuilder AddHtml(IRichTextContent content, IRichTextContentPartRendererCollection renderers, StringBuilder stringBuilder)
    {
        stringBuilder.Append("<ol>");

        RenderChildren(content, renderers, stringBuilder);

        stringBuilder.Append("</ol>");

        return stringBuilder;
    }
}
