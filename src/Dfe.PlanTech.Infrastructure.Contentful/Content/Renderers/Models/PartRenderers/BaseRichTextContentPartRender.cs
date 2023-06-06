using System.Text;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public abstract class BaseRichTextContentPartRender : IRichTextContentPartRenderer
{
    private readonly RichTextNodeType _nodeType;

    protected BaseRichTextContentPartRender(RichTextNodeType nodeType)
    {
        _nodeType = nodeType;
    }

    public bool Accepts(IRichTextContent content) => content.MappedNodeType == _nodeType;

    public abstract StringBuilder AddHtml(IRichTextContent content, IRichTextContentPartRendererCollection richTextRendererCollection, StringBuilder stringBuilder);

    public StringBuilder RenderChildren(IRichTextContent content, IRichTextContentPartRendererCollection renderers, StringBuilder stringBuilder)
    {
        foreach (var subContent in content.Content)
        {
            var renderer = renderers.GetRendererForContent(subContent);

            if (renderer == null)
            {
                //TODO: Add logging for missing content type
                continue;
            }

            renderer.AddHtml(subContent, renderers, stringBuilder);
        }

        return stringBuilder;
    }
}
