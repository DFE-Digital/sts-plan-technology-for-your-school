using System.Text;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content;

public abstract class RichTextContentRender : IRichTextContentPartRenderer
{
    private readonly RichTextNodeType _nodeType;

    protected RichTextContentRender(RichTextNodeType nodeType)
    {
        _nodeType = nodeType;
    }

    public bool Accepts(IRichTextContent content) => content.MappedNodeType == _nodeType;

    public abstract StringBuilder AddHtml(IRichTextContent content, IRichTextContentPartRendererCollection richTextRenderer, StringBuilder stringBuilder);
}
