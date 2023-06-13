
using System.Text;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class OrderedListRenderer : BaseRichTextContentPartRender<OrderedListRenderer>
{
    public OrderedListRenderer(ILogger<OrderedListRenderer> logger) : base(RichTextNodeType.OrderedList, logger)
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
