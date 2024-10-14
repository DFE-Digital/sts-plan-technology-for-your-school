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

        var external = IsExternalLink(content);
        if (external)
        {
            AddBlankTarget(stringBuilder);
        }

        stringBuilder.Append('>');

        rendererCollection.RenderChildren(content, stringBuilder);

        stringBuilder.Append(content.Value);

        if (external)
        {
            stringBuilder.Append(" (opens in new tab)");
        }

        stringBuilder.Append("</a>");
        return stringBuilder;
    }

    private static void AddTagAndHref(RichTextContent content, StringBuilder stringBuilder)
    {
        stringBuilder.Append("<a href=\"");
        stringBuilder.Append(content.Data?.Uri ?? "");
        stringBuilder.Append('"');
    }

    private static void AddBlankTarget(StringBuilder stringBuilder)
    {
        stringBuilder.Append("target=\"_blank\" rel=\"noopener\"");
    }

    private static bool IsExternalLink(RichTextContent content)
    {
        var uri = content.Data?.Uri ?? "";
        return uri.StartsWith("http", StringComparison.InvariantCultureIgnoreCase);
    }
}
