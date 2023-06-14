using System.Text;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

/// <summary>
/// Renders Rich Text content
/// </summary>
public interface IRichTextRenderer
{
    /// <summary>
    /// Renders all children of the content
    /// </summary>
    /// <param name="content"></param>
    /// <param name="stringBuilder"></param>
    public void RenderChildren(IRichTextContent content, StringBuilder stringBuilder);

    /// <summary>
    /// Converts content to HTML string
    /// </summary>
    /// <param name="content">Content to convert</param>
    /// <returns>Content converted to HTML string (including tags, classes, etc.)</returns>
    public string ToHtml(IRichTextContent content);
}
