using System.Text;
using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Contentful.Models.Options;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Rendering;

public class HyperlinkRenderer : BaseRichTextContentPartRenderer
{
    private readonly RichTextPartRendererOptions _options;

    public HyperlinkRenderer(RichTextPartRendererOptions options) : base(RichTextNodeType.Hyperlink)
    {
        _options = options;
    }

    public override StringBuilder AddHtml(RichTextContentField content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        if (string.IsNullOrEmpty(content.Data?.Uri))
        {
            rendererCollection.Logger.LogError("{This} has no Uri", this);
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

    private static void AddAttributes(RichTextContentField content, StringBuilder stringBuilder, bool isExternalLink)
    {
        stringBuilder.Append("href=\"");
        stringBuilder.Append(content.Data?.Uri ?? "");
        stringBuilder.Append('"');

        if (isExternalLink)
        {
            AddBlankTarget(stringBuilder);
        }
    }

    private static void AddLinkText(RichTextContentField content, StringBuilder stringBuilder, bool isExternalLink)
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

    private static bool IsExternalLink(RichTextContentField content)
    {
        var uri = content.Data?.Uri ?? "";
        return uri.StartsWith("http", StringComparison.InvariantCultureIgnoreCase);
    }
}
