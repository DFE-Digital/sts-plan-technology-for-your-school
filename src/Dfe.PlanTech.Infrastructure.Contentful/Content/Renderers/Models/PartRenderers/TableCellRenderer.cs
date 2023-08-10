using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using System.Text;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class TableCellRenderer : BaseRichTextContentPartRender
{
    public TableCellRenderer() : base(RichTextNodeType.TableCell)
    {
    }

    public override StringBuilder AddHtml(IRichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        
        String rowBeginsString = "<tr class=\"govuk-table__row\">";

        if (stringBuilder.ToString().EndsWith(rowBeginsString))
        {
            stringBuilder.Append("<th scope=\"row\" class=\"govuk-table__header\">");

            stringBuilder.Append(content.Content[0].Content[0].Value);

            stringBuilder.Append("</th>");
        }
        else
        {
            stringBuilder.Append("<td class=\"govuk-table__cell\">");

            stringBuilder.Append(content.Content[0].Content[0].Value);

            stringBuilder.Append("</td>");
        }


       

        return stringBuilder;
    }
}