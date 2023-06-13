using System.Text;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public abstract class BaseRichTextContentPartRender<TConcreteRenderer> : BaseRichTextRenderer<TConcreteRenderer>, IRichTextContentPartRenderer
{
    private readonly RichTextNodeType _nodeType;

    protected BaseRichTextContentPartRender(RichTextNodeType nodeType, ILogger<TConcreteRenderer> logger) : base(logger)
    {
        _nodeType = nodeType;
    }

    public bool Accepts(IRichTextContent content) => content.MappedNodeType == _nodeType;

    public abstract StringBuilder AddHtml(IRichTextContent content, IRichTextContentPartRendererCollection richTextRendererCollection, StringBuilder stringBuilder);
}
