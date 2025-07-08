using System.Text;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Data.Contentful.Content.Renderers.Models.PartRenderers;

public class TableRowRenderer : BaseRichTextContentPartRender
{
    public TableRowRenderer() : base(RichTextNodeType.TableRow)
    {
    }

    public override StringBuilder AddHtml(RichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        string bodyStartString = "<tbody class=\"govuk-table__body\">";

        bool isHeaderRow = (content.Content[0] as IRichTextContent).MappedNodeType == RichTextNodeType.TableHeaderCell;
        bool isFirstBodyRow = !stringBuilder.ToString().Contains(bodyStartString) && !isHeaderRow;

        if (isHeaderRow)
        {
            stringBuilder.Append("<thead class=\"govuk-table__head\">");
        }

        if (isFirstBodyRow)
        {
            stringBuilder.Append(bodyStartString);
        }

        stringBuilder.Append("<tr class=\"govuk-table__row\">");

        rendererCollection.RenderChildren(content, stringBuilder);

        stringBuilder.Append("</tr>");

        if (isHeaderRow)
        {
            stringBuilder.Append("</thead>");
        }

        return stringBuilder;
    }
}
