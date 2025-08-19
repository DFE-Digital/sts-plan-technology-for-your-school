using System.Text;
using Dfe.PlanTech.Application.Rendering.Options;
using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.Rendering;

public class ParagraphRenderer : BaseRichTextContentPartRenderer
{
    private readonly ParagraphRendererOptions _options;
    public ParagraphRenderer(ParagraphRendererOptions options) : base(RichTextNodeType.Paragraph)
    {
        _options = options;
    }

    public override StringBuilder AddHtml(RichTextContentField content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
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
