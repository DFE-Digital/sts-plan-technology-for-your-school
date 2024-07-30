using System.Text;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

public interface IRichTextContentPartRendererCollection
{
    public ILogger Logger { get; }

    /// <summary>
    /// Finds matching renderer for the given content, based on the content's node type
    /// </summary>
    /// <param name="content">Content to find renderer for</param>
    /// <returns>Matching part renderer for content (or null if not found)</returns>
    public IRichTextContentPartRenderer? GetRendererForContent(RichTextContent content);

    /// <summary>
    /// Renders all children of the content
    /// </summary>
    /// <param name="content"></param>
    /// <param name="stringBuilder"></param>
    public void RenderChildren(RichTextContent content, StringBuilder stringBuilder);
}
