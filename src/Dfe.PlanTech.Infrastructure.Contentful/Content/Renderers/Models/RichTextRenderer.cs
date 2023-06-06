using System.Text;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models;

/// <summary>
/// Parent class to render <see chref="IRichTextContent"/> RichTextContent
/// </summary>
public class RichTextRenderer : IRichTextRenderer, IRichTextContentPartRendererCollection
{
    private readonly List<IRichTextContentPartRenderer> _renderers;

    public RichTextRenderer(IEnumerable<IRichTextContentPartRenderer> renderers)
    {
        _renderers = renderers.ToList();
    }

    /// <summary>
    /// Finds matching renderer for the given content, based on the content's node type
    /// </summary>
    /// <param name="content">Content to find renderer for</param>
    /// <returns>Matching part renderer for content (or null if not found)</returns>
    public IRichTextContentPartRenderer? GetRendererForContent(IRichTextContent content)
    => _renderers.FirstOrDefault(renderer => renderer.Accepts(content));

    /// <summary>
    /// Converts content to HTML string
    /// </summary>
    /// <param name="content">Content to convert</param>
    /// <returns>Content converted to HTML string (including tags, classes, etc.)</returns>
    public string ToHtml(IRichTextContent content)
    {
        var stringBuilder = new StringBuilder();

        foreach (var subContent in content.Content)
        {
            var renderer = GetRendererForContent(subContent);

            if (renderer == null)
            {
                //TODO: Log missing content type
                continue;
            }

            renderer.AddHtml(subContent, this, stringBuilder);
        }

        return stringBuilder.ToString();
    }
}