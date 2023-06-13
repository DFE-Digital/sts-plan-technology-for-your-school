
using System.Text;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class ListItemRenderer : BaseRichTextContentPartRender<ListItemRenderer>
{
    public ListItemRenderer(ILogger<ListItemRenderer> logger) : base(RichTextNodeType.ListItem, logger)
    {
    }

    public override StringBuilder AddHtml(IRichTextContent content, IRichTextContentPartRendererCollection renderers, StringBuilder stringBuilder)
    {
        stringBuilder.Append("<li>");

        RenderChildren(content, renderers, stringBuilder);

        stringBuilder.Append("</li>");

        return stringBuilder;
    }
}