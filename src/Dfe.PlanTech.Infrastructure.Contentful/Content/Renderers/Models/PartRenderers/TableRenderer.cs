using System.Text;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class TableRenderer : BaseRichTextContentPartRender
{
    public TableRenderer() : base(RichTextNodeType.Table)
    {
    }

    public override StringBuilder AddHtml(RichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        stringBuilder.Append("<table class=\"govuk-table\">");

        rendererCollection.RenderChildren(content, stringBuilder);

        stringBuilder.Append("</tbody>");
        stringBuilder.Append("</table>");

        return stringBuilder;
    }

}
