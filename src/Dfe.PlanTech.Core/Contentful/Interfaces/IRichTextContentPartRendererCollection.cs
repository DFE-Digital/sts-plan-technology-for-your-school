using System.Text;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Core.Contentful.Interfaces;

public interface IRichTextContentPartRendererCollection
{
    public ILogger Logger { get; }

    IReadOnlyList<IRichTextContentPartRenderer> Renderers { get; }

    /// <summary>
    /// Finds matching renderer for the given content, based on the content's node type
    /// </summary>
    /// <param name="content">Content to find renderer for</param>
    /// <returns>Matching part renderer for content (or null if not found)</returns>
    public IRichTextContentPartRenderer? GetRendererForContent(CmsRichTextContentDto content);

    /// <summary>
    /// Renders all children of the content
    /// </summary>
    /// <param name="content"></param>
    /// <param name="stringBuilder"></param>
    public void RenderChildren(CmsRichTextContentDto content, StringBuilder stringBuilder);
}
