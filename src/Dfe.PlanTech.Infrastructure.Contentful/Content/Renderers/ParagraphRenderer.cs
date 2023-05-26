using System.Text;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content;

public class ParagraphRenderer : RichTextContentRender
{
    public ParagraphRenderer() : base(RichTextNodeType.Paragraph)
    {
    }

    public override StringBuilder AddHtml(IRichTextContent content, IRichTextContentPartRendererCollection richTextRenderer, StringBuilder stringBuilder)
    {
        stringBuilder.Append("<p>");

        foreach (var subContent in content.Content)
        {
            var renderer = richTextRenderer.GetRendererForContent(subContent);

            if (renderer == null)
            {
                //TODO: Add logging for missing content type
                continue;
            }

            stringBuilder.Append(renderer.AddHtml(subContent, richTextRenderer, stringBuilder));
        }

        stringBuilder.Append("</p>");
        return stringBuilder;
    }
}