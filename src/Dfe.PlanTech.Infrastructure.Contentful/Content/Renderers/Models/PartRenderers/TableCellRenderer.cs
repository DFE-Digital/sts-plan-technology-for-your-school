using System.Text;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class TableCellRenderer : BaseRichTextContentPartRender
{
    private const string BeginningOfRowString = "<tr class=\"govuk-table__row\">";
    private const string HeaderOpeningTag = "<th scope=\"row\" class=\"govuk-table__header\">";
    private const string CellOpeningTag = "<td class=\"govuk-table__cell\">";

    public TableCellRenderer() : base(RichTextNodeType.TableCell)
    {
    }

    public override StringBuilder AddHtml(RichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        if (stringBuilder.EndsWith(BeginningOfRowString))
        {
            stringBuilder.Append(HeaderOpeningTag);
            stringBuilder.Append(content.Content[0].Content[0].Value);
            stringBuilder.Append("</th>");
        }
        else
        {
            stringBuilder.Append(CellOpeningTag);
            stringBuilder.Append(content.Content[0].Content[0].Value);
            stringBuilder.Append("</td>");
        }

        return stringBuilder;
    }
}
