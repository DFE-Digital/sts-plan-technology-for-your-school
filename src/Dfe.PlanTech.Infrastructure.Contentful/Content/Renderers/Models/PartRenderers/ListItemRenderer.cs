
using System.Text;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class ListItemRenderer : BaseRichTextContentPartRender
{
    public ListItemRenderer() : base(RichTextNodeType.ListItem)
    {
    }

    public override StringBuilder AddHtml(IRichTextContent content, IRichTextContentPartRendererCollection renderers, StringBuilder stringBuilder)
    {
        stringBuilder.Append("<li>");

        renderers.RenderChildren(content, stringBuilder);

        stringBuilder.Append("</li>");

        return stringBuilder;
    }
}