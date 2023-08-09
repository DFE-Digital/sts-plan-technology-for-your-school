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
        stringBuilder.Append("<td>");

        stringBuilder.Append(content.Content[0].Content[0].Value);

        stringBuilder.Append("</td>");

        return stringBuilder;
    }
}