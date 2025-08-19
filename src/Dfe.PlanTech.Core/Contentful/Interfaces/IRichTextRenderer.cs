using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.Contentful.Interfaces;

/// <summary>
/// Renders Rich Text content
/// </summary>
public interface IRichTextRenderer
{
    /// <summary>
    /// Converts content to HTML string
    /// </summary>
    /// <param name="content">Content to convert</param>
    /// <returns>Content converted to HTML string (including tags, classes, etc.)</returns>
    public string ToHtml(RichTextContentField content);
}
