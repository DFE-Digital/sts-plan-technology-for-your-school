using System.Text;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.Contentful.Interfaces;

public interface IRichTextContentPartRenderer
{
    /// <summary>
    /// Whether this PartRenderer can render this content
    /// </summary>
    /// <param name="content">Content to check for acceptance</param>
    /// <returns>True (can render), false (can't render)</returns>
    public bool Accepts(IRichTextContent content);

    /// <summary>
    /// Converts content to HTML string, and adds to string builder
    /// </summary>
    /// <param name="content"></param>
    /// <param name="rendererCollection"></param>
    /// <param name="stringBuilder"></param>
    /// <returns></returns>
    public StringBuilder AddHtml(
        RichTextContentField content,
        IRichTextContentPartRendererCollection rendererCollection,
        StringBuilder stringBuilder
    );
}
