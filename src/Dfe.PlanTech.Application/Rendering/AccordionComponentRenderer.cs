using System.Text;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Rendering;

public class AccordionComponentRenderer(
    ILogger<AccordionComponentRenderer> logger
) : IRichTextContentPartRendererCollection
{
    private readonly ILogger<AccordionComponentRenderer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    public ILogger Logger => _logger;
    public IReadOnlyList<IRichTextContentPartRenderer> Renderers { get; private set; } = new List<IRichTextContentPartRenderer>();

    public StringBuilder AddHtml(RichTextContentField content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        Renderers = rendererCollection.Renderers;

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

    public void RenderChildren(RichTextContentField content, StringBuilder stringBuilder)
    {
        foreach (var subContent in content.Content)
        {
            var renderer = GetRendererForContent(subContent);

            if (renderer == null)
            {
                Logger.LogWarning("Could not find renderer for {SubContent}", subContent);
                continue;
            }

            renderer.AddHtml(subContent, this, stringBuilder);
        }
    }

    public IRichTextContentPartRenderer? GetRendererForContent(RichTextContentField content)
    => Renderers.FirstOrDefault(renderer => renderer.Accepts(content));
}
