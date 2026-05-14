using System.Text;
using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Contentful.Rendering;

namespace Dfe.PlanTech.Application.Rendering.Contentful;

public class TableRenderer : BaseRichTextContentPartRenderer
{
    public TableRenderer()
        : base(RichTextNodeType.Table) { }

    public override StringBuilder AddHtml(
        RichTextContentField content,
        IRichTextContentPartRendererCollection rendererCollection,
        StringBuilder stringBuilder
    )
    {
        stringBuilder.Append("<table class=\"govuk-table\">");

        rendererCollection.RenderChildren(content, stringBuilder);

        stringBuilder.Append("</tbody>");
        stringBuilder.Append("</table>");

        return stringBuilder;
    }
}
