using System.Text;
using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Extensions;

namespace Dfe.PlanTech.Application.Rendering;

public class TableCellRenderer : BaseRichTextContentPartRenderer
{
    private const string BeginningOfRowString = "<tr class=\"govuk-table__row\">";
    private const string HeaderOpeningTag = "<th scope=\"row\" class=\"govuk-table__header\">";
    private const string CellOpeningTag = "<td class=\"govuk-table__cell\">";

    public TableCellRenderer() : base(RichTextNodeType.TableCell)
    {
    }

    public override StringBuilder AddHtml(RichTextContentField content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
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

    private static void AppendContent(StringBuilder stringBuilder, List<RichTextContentField> content)
    {
        for (int i = 0; i < content.Count; i++)
        {
            stringBuilder.Append(content[i].Content[0].Value);

            if (i < content.Count - 1)
            {
                stringBuilder.Append("<br /><br />");
            }
        }
    }
}
