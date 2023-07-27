using Dfe.PlanTech.Domain.Content.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models;

/// <summary>
/// Parent class to render <see chref="IRichTextContent"/> RichTextContent
/// </summary>
public class RichTextRenderer : IRichTextRenderer, IRichTextContentPartRendererCollection
{
    private readonly ILogger<IRichTextRenderer> _logger;
    private readonly List<IRichTextContentPartRenderer> _renderers;

    public ILogger Logger => _logger;

    public RichTextRenderer(ILogger<IRichTextRenderer> logger, IEnumerable<IRichTextContentPartRenderer> renderers)
    {
        _logger = logger;
        _renderers = renderers.ToList();
    }

    public string ToHtml(IRichTextContent content)
    {
        var stringBuilder = new StringBuilder();

        RenderChildren(content, stringBuilder);

        return stringBuilder.ToString();
    }

    public void RenderChildren(IRichTextContent content, StringBuilder stringBuilder)
    {
        foreach (var subContent in content.Content)
        {
            var renderer = GetRendererForContent(subContent);

            if (renderer == null)
            {
                _logger.LogWarning("Could not find renderer for {subContent}", subContent);
                continue;
            }

            renderer.AddHtml(subContent, this, stringBuilder);
        }
    }


    /// <summary>
    /// Finds matching renderer for the given content, based on the content's node type
    /// </summary>
    /// <param name="content">Content to find renderer for</param>
    /// <returns>Matching part renderer for content (or null if not found)</returns>
    public IRichTextContentPartRenderer? GetRendererForContent(IRichTextContent content)
    => _renderers.FirstOrDefault(renderer => renderer.Accepts(content));
}