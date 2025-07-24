using System.Text;
using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Application.Rendering;

public abstract class BaseRichTextContentPartRenderer : IRichTextContentPartRenderer
{
    private readonly RichTextNodeType _nodeType;

    protected BaseRichTextContentPartRenderer(RichTextNodeType nodeType)
    {
        _nodeType = nodeType;
    }

    public bool Accepts(IRichTextContent content) =>
        content.MappedNodeType == _nodeType;

    public abstract StringBuilder AddHtml(CmsRichTextContentDto content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder);
}
