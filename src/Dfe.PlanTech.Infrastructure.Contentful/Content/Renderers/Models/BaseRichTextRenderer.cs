using System.Text;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models;

public abstract class BaseRichTextRenderer<TConcreteRenderer>
{
    protected readonly ILogger<TConcreteRenderer> logger;

    protected BaseRichTextRenderer(ILogger<TConcreteRenderer> logger)
    {
        this.logger = logger;
    }

    public StringBuilder RenderChildren(IRichTextContent content, IRichTextContentPartRendererCollection renderers, StringBuilder stringBuilder)
    {
        foreach (var subContent in content.Content)
        {
            var renderer = renderers.GetRendererForContent(subContent);

            if (renderer == null)
            {
                logger.LogWarning("Could not find renderer for {subContent}", subContent);
                continue;
            }

            renderer.AddHtml(subContent, renderers, stringBuilder);
        }

        return stringBuilder;
    }
}
