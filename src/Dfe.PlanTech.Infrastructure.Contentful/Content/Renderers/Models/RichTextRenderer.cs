using System.Text;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models;

/// <summary>
/// Parent class to render <see chref="IRichTextContent"/> RichTextContent
/// </summary>
/// <inheritdoc/>
public class RichTextRenderer : IRichTextRenderer, IRichTextContentPartRendererCollection
{
    private readonly ILogger<RichTextRenderer> _logger;
    private readonly List<IRichTextContentPartRenderer> _renderers;

    public ILogger Logger => _logger;

    public RichTextRenderer(ILogger<RichTextRenderer> logger, IEnumerable<IRichTextContentPartRenderer> renderers)
    {
        _logger = logger;
        _renderers = renderers.ToList();
    }

    public string ToHtml(RichTextContent content)
    {
        var stringBuilder = new StringBuilder();

        RenderChildren(content, stringBuilder);

        return stringBuilder.ToString();
    }

    public void RenderChildren(RichTextContent content, StringBuilder stringBuilder)
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

    public IRichTextContentPartRenderer? GetRendererForContent(RichTextContent content)
    => _renderers.FirstOrDefault(renderer => renderer.Accepts(content));
}
