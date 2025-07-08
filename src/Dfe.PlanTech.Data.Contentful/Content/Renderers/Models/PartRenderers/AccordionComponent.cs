using System.Text;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Data.Contentful.Content.Renderers.Models.PartRenderers;

public class AccordionComponent : IRichTextContentPartRendererCollection
{
    private readonly ILogger<AccordionComponent> _logger;
    public ILogger Logger => _logger;
    public IReadOnlyList<IRichTextContentPartRenderer> Renders { get; private set; } = new List<IRichTextContentPartRenderer>();
    public AccordionComponent(ILogger<AccordionComponent> logger)
    {
        _logger = logger;
    }

    public StringBuilder AddHtml(RichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        Renders = rendererCollection.Renders;

        var nestedContent = content?.Data?.Target?.Content ?? null;
        if (nestedContent != null && nestedContent.Any())
        {
            stringBuilder.AppendLine($"<div class=\"govuk-accordion\" data-module=\"govuk-accordion\" id=\"accordion-{nestedContent[0].InternalName}\">");
            foreach (var innerContent in nestedContent)
            {
                stringBuilder.Append("<div class=\"govuk-accordion__section govuk-body\">");
                stringBuilder.Append("<div class=\"govuk-accordion__section-header\">");
                stringBuilder.Append("<h2 class=\"govuk-accordion__section-heading\">");
                stringBuilder.Append($"<span class=\"govuk-accordion__section-button\" id=\"{innerContent.InternalName}-heading\">");
                stringBuilder.Append(innerContent.Title);
                stringBuilder.Append("</span></h2>");
                stringBuilder.Append($"<div class=\"govuk-accordion__section-summary govuk-body\" id=\"{innerContent.InternalName}-summary\">");
                stringBuilder.Append(innerContent.SummaryLine);
                stringBuilder.Append("</div></div>");
                stringBuilder.Append($"<div id=\"{innerContent.InternalName}-content\" class=\"govuk-accordion__section-content\">");

                RenderChildren(innerContent.RichText, stringBuilder);

                stringBuilder.Append("</div></div>");
            }
            stringBuilder.Append("</div>");
        }
        return stringBuilder;
    }

    public void RenderChildren(RichTextContent content, StringBuilder stringBuilder)
    {
        foreach (var subContent in content.Content)
        {
            var renderer = GetRendererForContent(subContent);

            if (renderer == null)
            {
                Logger.LogWarning("Could not find renderer for {subContent}", subContent);
                continue;
            }

            renderer.AddHtml(subContent, this, stringBuilder);
        }
    }

    public IRichTextContentPartRenderer? GetRendererForContent(RichTextContent content)
    => Renders.FirstOrDefault(renderer => renderer.Accepts(content));
}
