
using System.Text;
using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.Rendering;

public class UnorderedListRenderer : BaseRichTextContentPartRenderer
{
    public UnorderedListRenderer() : base(RichTextNodeType.UnorderedList)
    {
    }

    public override StringBuilder AddHtml(RichTextContentField content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        stringBuilder.Append("<ul>");

        rendererCollection.RenderChildren(content, stringBuilder);

        stringBuilder.Append("</ul>");

        return stringBuilder;
    }
}
