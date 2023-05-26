
using System.Text;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Interfaces;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class TextRenderer : RichTextContentRender
{
    private readonly TextRendererOptions _textRendererOptions;

    public TextRenderer(TextRendererOptions textRendererOptions) : base(RichTextNodeType.Text)
    {
        _textRendererOptions = textRendererOptions;
    }

    public override StringBuilder AddHtml(IRichTextContent content, IRichTextContentPartRendererCollection richTextRenderer, StringBuilder stringBuilder)
    {
        var marks = content.Marks.Select(_textRendererOptions.GetHtmlForMark).ToArray();

        foreach (var mark in marks)
        {
            stringBuilder.Append('<');
            stringBuilder.Append(mark);
            stringBuilder.Append('>');
        }

        stringBuilder.Append(content.Value);

        foreach (var mark in marks)
        {
            stringBuilder.Append("</");
            stringBuilder.Append(mark);
            stringBuilder.Append('>');
        }

        return stringBuilder;
    }
}
