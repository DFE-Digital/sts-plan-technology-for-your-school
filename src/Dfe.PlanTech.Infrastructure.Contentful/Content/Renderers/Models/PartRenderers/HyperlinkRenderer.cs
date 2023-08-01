using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Options;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class HyperlinkRenderer : BaseRichTextContentPartRender
{
    private readonly RichTextPartRendererOptions _options;

    public HyperlinkRenderer(RichTextPartRendererOptions options) : base(RichTextNodeType.Hyperlink)
    {
        _options = options;
    }

    public override StringBuilder AddHtml(IRichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        if (string.IsNullOrEmpty(content.Data?.Uri))
        {
            rendererCollection.Logger.LogError("{this} has no Uri", this);
            return stringBuilder;
        }

        stringBuilder.Append("<a href=\"");
        stringBuilder.Append(content.Data.Uri);

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
}