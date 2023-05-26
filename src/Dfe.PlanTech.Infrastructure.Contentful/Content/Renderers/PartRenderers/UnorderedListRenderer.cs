
using System.Text;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.PartRenderers;

public class UnorderedListRenderer : RichTextContentRender
{
    public UnorderedListRenderer() : base(RichTextNodeType.UnorderedList)
    {
    }

    public override StringBuilder AddHtml(IRichTextContent content, IRichTextContentPartRendererCollection renderers, StringBuilder stringBuilder)
    {
        stringBuilder.Append("<ul>");

        RenderChildren(content, renderers, stringBuilder);

        stringBuilder.Append("</ul>");

        return stringBuilder;
    }
}
