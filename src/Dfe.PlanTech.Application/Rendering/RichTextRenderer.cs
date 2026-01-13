using System.Text;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Rendering;

/// <summary>
/// Parent class to render <see chref="IRichTextContent"/> RichTextContent
/// </summary>
/// <inheritdoc/>
public class RichTextRenderer(
    ILogger<RichTextRenderer> logger,
    IEnumerable<IRichTextContentPartRenderer> renderers
) : IRichTextRenderer, IRichTextContentPartRendererCollection
{
    public IReadOnlyList<IRichTextContentPartRenderer> Renderers => renderers.ToList();

    private readonly ILogger<RichTextRenderer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public ILogger Logger => _logger;

    public string ToHtml(RichTextContentField content)
    {
        var stringBuilder = new StringBuilder();

        RenderChildren(content, stringBuilder);

        return stringBuilder.ToString();
    }

    public void RenderChildren(RichTextContentField content, StringBuilder stringBuilder)
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

    public IRichTextContentPartRenderer? GetRendererForContent(RichTextContentField content)
        => Renderers.FirstOrDefault(renderer => renderer.Accepts(content));
}
