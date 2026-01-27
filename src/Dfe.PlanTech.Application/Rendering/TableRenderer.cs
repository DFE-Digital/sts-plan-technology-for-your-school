using System.Text;
using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.Rendering;

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
