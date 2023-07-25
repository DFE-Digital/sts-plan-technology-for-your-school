
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using System.Text;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class UnorderedListRenderer : BaseRichTextContentPartRender
{
    public UnorderedListRenderer() : base(RichTextNodeType.UnorderedList)
    {
    }

    public override StringBuilder AddHtml(IRichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        stringBuilder.Append("<ul>");

        rendererCollection.RenderChildren(content, stringBuilder);

        stringBuilder.Append("</ul>");

        return stringBuilder;
    }
}
