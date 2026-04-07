using System.Text;
using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Contentful.Rendering;

namespace Dfe.PlanTech.Application.Rendering.Contentful;

public abstract class BaseRichTextContentPartRenderer(RichTextNodeType nodeType)
    : IRichTextContentPartRenderer
{
    private readonly RichTextNodeType _nodeType = nodeType;

    public bool Accepts(IRichTextContent content) => content.MappedNodeType == _nodeType;

    public abstract StringBuilder AddHtml(
        RichTextContentField content,
        IRichTextContentPartRendererCollection rendererCollection,
        StringBuilder stringBuilder
    );
}
