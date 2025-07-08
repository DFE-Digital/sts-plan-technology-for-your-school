
using System.Text;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Data.Contentful.Content.Renderers.Models.PartRenderers;

public class UnorderedListRenderer : BaseRichTextContentPartRender
{
    public UnorderedListRenderer() : base(RichTextNodeType.UnorderedList)
    {
    }

    public override StringBuilder AddHtml(RichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        stringBuilder.Append("<ul>");

        rendererCollection.RenderChildren(content, stringBuilder);

        stringBuilder.Append("</ul>");

        return stringBuilder;
    }
}
