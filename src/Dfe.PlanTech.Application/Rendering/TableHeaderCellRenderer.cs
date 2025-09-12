using System.Text;
using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.Rendering;

public class TableHeaderCellRenderer : BaseRichTextContentPartRenderer
{
    public TableHeaderCellRenderer() : base(RichTextNodeType.TableHeaderCell)
    {
    }

    public override StringBuilder AddHtml(RichTextContentField content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        stringBuilder.Append("<th class=\"govuk-table__header\">");

        stringBuilder.Append(content.Content[0].Content[0].Value);

        stringBuilder.Append("</th>");

        return stringBuilder;
    }
}
