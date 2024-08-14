using System.Text;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Options;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class HyperlinkRenderer : BaseRichTextContentPartRender
{
    private readonly HyperlinkRendererOptions _options;

    public HyperlinkRenderer(HyperlinkRendererOptions options) : base(RichTextNodeType.Hyperlink)
    {
        _options = options;
    }

    public override StringBuilder AddHtml(RichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        if (string.IsNullOrEmpty(content.Data?.Uri))
        {
            rendererCollection.Logger.LogError("{this} has no Uri", this);
            return stringBuilder;
        }

        AddTagAndHref(content, stringBuilder);

        if (_options.Classes != null)
        {
            _options.AddClasses(stringBuilder);
        }

        stringBuilder.Append("\">");

        rendererCollection.RenderChildren(content, stringBuilder);

        stringBuilder.Append(content.Value);

        stringBuilder.Append("</a>");
        return stringBuilder;
    }

    private static void AddTagAndHref(RichTextContent content, StringBuilder stringBuilder)
    {
        stringBuilder.Append("<a href=\"");
        stringBuilder.Append(content.Data?.Uri ?? "");
        stringBuilder.Append('"');
    }
}
