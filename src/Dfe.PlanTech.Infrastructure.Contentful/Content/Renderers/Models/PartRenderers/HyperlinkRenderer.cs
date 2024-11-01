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

        stringBuilder.Append("<a ");

        var external = IsExternalLink(content);
        AddAttributes(content, stringBuilder, external);

        if (_options.Classes != null)
        {
            _options.AddClasses(stringBuilder);
        }

        stringBuilder.Append('>');

        rendererCollection.RenderChildren(content, stringBuilder);

        AddLinkText(content, stringBuilder, external);

        stringBuilder.Append("</a>");
        return stringBuilder;
    }

    private static void AddAttributes(RichTextContent content, StringBuilder stringBuilder, bool isExternalLink)
    {
        stringBuilder.Append("href=\"");
        stringBuilder.Append(content.Data?.Uri ?? "");
        stringBuilder.Append('"');

        if (isExternalLink)
        {
            AddBlankTarget(stringBuilder);
        }
    }

    private static void AddLinkText(RichTextContent content, StringBuilder stringBuilder, bool isExternalLink)
    {
        stringBuilder.Append(content.Value);

        if (isExternalLink)
        {
            stringBuilder.Append(" (opens in new tab)");
        }
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
