using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using System.Text;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class TableRowRenderer : BaseRichTextContentPartRender
{
    public TableRowRenderer() : base(RichTextNodeType.TableRow)
    {
    }

    public override StringBuilder AddHtml(IRichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        bool isHeaderRow = content.Content[0].MappedNodeType == RichTextNodeType.TableHeaderCell;
        
        if (isHeaderRow)
        {
            stringBuilder.Append("<thead class=\"govuk-table__head\">");
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