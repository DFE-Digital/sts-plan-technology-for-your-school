using System.Text;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public abstract class BaseRichTextContentPartRender : IRichTextContentPartRenderer
{
    private readonly RichTextNodeType _nodeType;

    protected BaseRichTextContentPartRender(RichTextNodeType nodeType)
    {
        _nodeType = nodeType;
    }

    public bool Accepts(IRichTextContent content) => content.MappedNodeType == _nodeType;

    public abstract StringBuilder AddHtml(IRichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder);
}
