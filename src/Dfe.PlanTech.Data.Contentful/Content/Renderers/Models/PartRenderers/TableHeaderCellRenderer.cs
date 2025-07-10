using System.Text;
using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Data.Contentful.Content.Renderers.Models.PartRenderers;

public class TableHeaderCellRenderer : BaseRichTextContentPartRender
{
    public TableHeaderCellRenderer() : base(RichTextNodeType.TableHeaderCell)
    {
    }

    public override StringBuilder AddHtml(RichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        stringBuilder.Append("<th class=\"govuk-table__header\">");

        stringBuilder.Append(content.Content[0].Content[0].Value);

        stringBuilder.Append("</th>");

        return stringBuilder;
    }
}
