
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using System.Text;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class ListItemRenderer : BaseRichTextContentPartRender
{
    public ListItemRenderer() : base(RichTextNodeType.ListItem)
    {
    }

    public override StringBuilder AddHtml(IRichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        stringBuilder.Append("<li>");

        if (content != null && content.Content.Length > 0 && content.Content[0].Content.Length > 0)
        {
            stringBuilder.Append(content.Content[0].Content[0].Value);
        }
        stringBuilder.Append("</li>");

        return stringBuilder;
    }
}