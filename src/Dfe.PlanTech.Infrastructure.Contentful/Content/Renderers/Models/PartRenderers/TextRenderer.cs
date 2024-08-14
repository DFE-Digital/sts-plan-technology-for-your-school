
using System.Text;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Options;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class TextRenderer : BaseRichTextContentPartRender
{
    private readonly TextRendererOptions _textRendererOptions;

    public TextRenderer(TextRendererOptions textRendererOptions) : base(RichTextNodeType.Text)
    {
        _textRendererOptions = textRendererOptions;
    }

    public override StringBuilder AddHtml(RichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        var markOptions = content.Marks.Select(_textRendererOptions.GetMatchingOptionForMark)
                                        .Where(option => option != null)
                                        .ToArray();

        AppendOpenTags(stringBuilder, markOptions!);

        stringBuilder.Append(content.Value);

        AppendCloseTags(stringBuilder, markOptions!);

        return stringBuilder;
    }

    private static void AppendCloseTags(StringBuilder stringBuilder, IEnumerable<MarkOption> markOptions)
    {
        foreach (var mark in markOptions)
        {
            stringBuilder.Append("</");
            stringBuilder.Append(mark.HtmlTag);
            stringBuilder.Append('>');
        }
    }

    private static void AppendOpenTags(StringBuilder stringBuilder, IEnumerable<MarkOption> markOptions)
    {
        foreach (var mark in markOptions)
        {
            stringBuilder.Append('<');
            foreach (var htmlPart in TextRendererOptions.GetOpenTagHtml(mark))
            {
                stringBuilder.Append(htmlPart);
            }
            stringBuilder.Append('>');
        }
    }
}
