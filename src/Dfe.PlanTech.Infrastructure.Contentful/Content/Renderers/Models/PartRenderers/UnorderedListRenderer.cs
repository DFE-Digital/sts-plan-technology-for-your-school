
using System.Text;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class UnorderedListRenderer : BaseRichTextContentPartRender<UnorderedListRenderer>
{
    public UnorderedListRenderer(ILogger<UnorderedListRenderer> logger) : base(RichTextNodeType.UnorderedList, logger)
    {
    }

    public override StringBuilder AddHtml(IRichTextContent content, IRichTextContentPartRendererCollection renderers, StringBuilder stringBuilder)
    {
        stringBuilder.Append("<ul>");

        RenderChildren(content, renderers, stringBuilder);

        stringBuilder.Append("</ul>");

        return stringBuilder;
    }
}
