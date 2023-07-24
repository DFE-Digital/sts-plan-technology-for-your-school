using Microsoft.Extensions.Logging;
using System.Text;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

public interface IRichTextContentPartRendererCollection
{
    public ILogger Logger { get; }

    public IRichTextContentPartRenderer? GetRendererForContent(IRichTextContent content);

    /// <summary>
    /// Renders all children of the content
    /// </summary>
    /// <param name="content"></param>
    /// <param name="stringBuilder"></param>
    public void RenderChildren(IRichTextContent content, StringBuilder stringBuilder);
}