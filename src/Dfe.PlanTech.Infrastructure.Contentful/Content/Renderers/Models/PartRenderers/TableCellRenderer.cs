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
            AppendContent(stringBuilder, content.Content);
            stringBuilder.Append("</th>");
        }
        else
        {
            stringBuilder.Append(CellOpeningTag);
            AppendContent(stringBuilder, content.Content);
            stringBuilder.Append("</td>");
        }

        return stringBuilder;
    }

    private StringBuilder AppendContent(StringBuilder stringBuilder, List<RichTextContent> content)
    {
        if (content.Count == 0)
        {
            return stringBuilder;
        }

        for (int i=0; i < content.Count; i++)
        {
            stringBuilder.Append(content[i].Content[0].Value);

            if (i < content.Count - 1)
            {
                stringBuilder.Append("<br /><br />");
            }
        };

        return stringBuilder;
    }
}
