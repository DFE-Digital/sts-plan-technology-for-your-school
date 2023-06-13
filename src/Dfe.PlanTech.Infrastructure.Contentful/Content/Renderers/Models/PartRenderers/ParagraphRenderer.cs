using System.Text;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Options;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class ParagraphRenderer : BaseRichTextContentPartRender<ParagraphRenderer>
{
    private readonly ParagraphRendererOptions _options;
    public ParagraphRenderer(ParagraphRendererOptions options, ILogger<ParagraphRenderer> logger) : base(RichTextNodeType.Paragraph, logger)
    {
        _options = options;
    }

    public override StringBuilder AddHtml(IRichTextContent content, IRichTextContentPartRendererCollection renderers, StringBuilder stringBuilder)
    {
        if (_options.Classes == null)
        {
            stringBuilder.Append("<p>");
        }
        else
        {
            stringBuilder.Append("<p");
            _options.AddClasses(stringBuilder);
            stringBuilder.Append("\">");
        }

        RenderChildren(content, renderers, stringBuilder);

        stringBuilder.Append("</p>");
        return stringBuilder;
    }
}