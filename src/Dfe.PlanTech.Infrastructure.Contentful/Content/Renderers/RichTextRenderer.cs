using System.Text;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content;

public class RichTextRenderer : IRichTextRenderer, IRichTextContentPartRendererCollection
{
    private readonly List<IRichTextContentPartRenderer> _renderers;

    public RichTextRenderer(IEnumerable<IRichTextContentPartRenderer> renderers)
    {
        _renderers = renderers.ToList();
    }

    public IRichTextContentPartRenderer? GetRendererForContent(IRichTextContent content)
    => _renderers.FirstOrDefault(renderer => renderer.Accepts(content));

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