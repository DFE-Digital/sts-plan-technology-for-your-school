
using System.Text;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class ListItemRenderer : BaseRichTextContentPartRender
{
    public ListItemRenderer() : base(RichTextNodeType.ListItem)
    {
    }

    public override StringBuilder AddHtml(RichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        stringBuilder.Append("<li>");

        rendererCollection.RenderChildren(content, stringBuilder);

        stringBuilder.Append("</li>");

        return stringBuilder;
    }
}
