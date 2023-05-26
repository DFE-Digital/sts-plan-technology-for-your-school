using System.Text;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Options;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content;

public class ParagraphRenderer : RichTextContentRender
{
    private readonly ParagraphRendererOptions _options;
    public ParagraphRenderer(ParagraphRendererOptions options) : base(RichTextNodeType.Paragraph)
    {
        _options = options;
    }

    public override StringBuilder AddHtml(IRichTextContent content, IRichTextContentPartRendererCollection richTextRenderer, StringBuilder stringBuilder)
    {
        if (_options.Classes == null)
        {
            stringBuilder.Append("<p>");
        }
        else
        {
            stringBuilder.Append("<p class=\"");
            stringBuilder.Append(_options.Classes);
            stringBuilder.Append("\">");
        }

        foreach (var subContent in content.Content)
        {
            var renderer = richTextRenderer.GetRendererForContent(subContent);

            if (renderer == null)
            {
                //TODO: Add logging for missing content type
                continue;
            }

            renderer.AddHtml(subContent, richTextRenderer, stringBuilder);
        }

        stringBuilder.Append("</p>");
        return stringBuilder;
    }
}