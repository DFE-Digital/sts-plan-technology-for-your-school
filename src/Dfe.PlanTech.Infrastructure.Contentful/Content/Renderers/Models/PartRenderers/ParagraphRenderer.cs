using System.Text;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Options;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class ParagraphRenderer : BaseRichTextContentPartRender
{
    private readonly ParagraphRendererOptions _options;
    public ParagraphRenderer(ParagraphRendererOptions options) : base(RichTextNodeType.Paragraph)
    {
        _options = options;
    }

    public override StringBuilder AddHtml(RichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        if (_options.Classes == null)
        {
            stringBuilder.Append("<p>");
        }
        else
        {
            stringBuilder.Append("<p");
            _options.AddClasses(stringBuilder);
            stringBuilder.Append('>');
        }

        rendererCollection.RenderChildren(content, stringBuilder);

        stringBuilder.Append("</p>");
        return stringBuilder;
    }
}
